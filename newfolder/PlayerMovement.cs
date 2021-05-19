using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Rigidbody2D _rb2D;
    private BoxCollider2D _boxCollider2D;
    private GameObject _crosshair;
    private Vector2 _moveAxisValue;
    private Vector2 _lastMoveAxisValue = new Vector2(1, 1);
    private Vector2 _footOffset_L;
    private Vector2 _footOffset_M;
    private Vector2 _footOffset_R;
    private Vector2 _slopeDir;
    private Vector2 _newVelocity;

    private float _runSpeed = 15.0f;
    private float _maxJumpHeight = 1.75f;
    private float _maxJumpTime = 0.6f;
    private float _jumpStartTime;
    private float _fallStartTime;
    private float _fallStartYSpeed;
    private float _maxFallSpeed = -15.0f;
    private float _maxFallSpeedTime = 0.6f;

    private bool _isRunning;
    private bool _isJumping;
    private bool _isFalling;
    private bool _isGrounded;
    private bool _isOnSlope;
    //private bool _isRolling;
    //private bool _isCrouching;
    private bool _isAiming;
    //private bool _isMenuOpened;

    private string _moveStr = "Move";
    private string _jumpStr_Keyboard = "Jump_Keyboard";
    private string _jumpStr_Gamepad = "Jump_Gamepad";
    private string _item1 = "Item1";
    private string _item2 = "Item2";
    private string _item3 = "Item3";
    private string _switchItem = "SwitchItem";
    private string _switchBullet = "SwitchBullet";
    private string _switchMeleeWeapon = "SwitchMeleeWeapon";
    private string _switchMoveMode = "SwitchMoveMode";
    private string _aimStart = "Aim_Start";
    private string _aimAxis = "Aim_Axis";
    private string _dash = "Dash";
    private string _attack_Shoot = "Attack_Shoot";
    private string _attack_Melee = "Attack_Melee";
    private string _crouchRoll = "CrouchOrRoll";
    private string _pauseMenu = "PauseMenu_Open";

    private InputActionAsset iaa;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _rb2D = GetComponent<Rigidbody2D>();
        _crosshair = transform.parent.Find("Player-Irreversible/Crosshair").gameObject;
        _boxCollider2D = transform.Find("PlayerBody").GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        _footOffset_L = new Vector2(-_boxCollider2D.size.x / 2 * Mathf.Abs(_boxCollider2D.transform.localScale.x), -_boxCollider2D.size.y / 2 * Mathf.Abs(_boxCollider2D.transform.localScale.y));
        _footOffset_M = new Vector2(0, -_boxCollider2D.size.y / 2 * Mathf.Abs(_boxCollider2D.transform.localScale.y));
        _footOffset_R = new Vector2(_boxCollider2D.size.x / 2 * Mathf.Abs(_boxCollider2D.transform.localScale.x), -_boxCollider2D.size.y / 2 * Mathf.Abs(_boxCollider2D.transform.localScale.y));
    }

    private void OnEnable()
    {
        //_playerInput.actions[_moveStr].started += Move_Started;
        _playerInput.actions[_moveStr].performed += Move_Performed;
        _playerInput.actions[_moveStr].canceled += Move_Canceled;
        _playerInput.actions[_jumpStr_Keyboard].started += Jump_Started;
        _playerInput.actions[_jumpStr_Keyboard].performed += Jump_Performed;
        _playerInput.actions[_jumpStr_Keyboard].canceled += Jump_Canceled;
        _playerInput.actions[_jumpStr_Gamepad].started += Jump_Started;
        _playerInput.actions[_jumpStr_Gamepad].performed += Jump_Performed;
        _playerInput.actions[_jumpStr_Gamepad].canceled += Jump_Canceled;
        _playerInput.actions[_item1].performed += ctx => UseItem_Performed(ctx, 1);
        _playerInput.actions[_item2].performed += ctx => UseItem_Performed(ctx, 2);
        _playerInput.actions[_item3].performed += ctx => UseItem_Performed(ctx, 3);
        _playerInput.actions[_switchItem].performed += SwitchItem_Performed;
        _playerInput.actions[_switchBullet].performed += SwitchBullet_Performed;
        _playerInput.actions[_switchMeleeWeapon].performed += SwitchMeleeWeapon_Performed;
        _playerInput.actions[_switchMoveMode].performed += SwitchMoveMode_Performed;
        _playerInput.actions[_aimStart].performed += AimStart_Performed;
        _playerInput.actions[_aimStart].canceled += AimStart_Canceled;
        _playerInput.actions[_aimAxis].performed += AimAxis_Performed;
        _playerInput.actions[_dash].performed += Dash_Performed;
        _playerInput.actions[_attack_Shoot].performed += Attack_Shoot_Performed;
        _playerInput.actions[_attack_Melee].performed += Attack_Melee_Performed;
        _playerInput.actions[_crouchRoll].performed += CrouchOrRoll_Performed;
        _playerInput.actions[_pauseMenu].performed += PauseMenu_Performed;
    }

    private void OnDisable()
    {

    }

    void Update()
    {
        DeterminePlayerStatus();
        //Debug.Log(_isGrounded);
    }

    private void FixedUpdate()
    {
        _newVelocity = Vector2.zero;

        if (_isRunning) _newVelocity += CalcRunSpeed();
        if (_isJumping) _newVelocity += CalcJumpSpeed();
        if (_isFalling) _newVelocity += CalcFallSpeed();

        _rb2D.velocity = _newVelocity;
    }

    //private void Move_Started(InputAction.CallbackContext ctx)
    //{

    //}

    private void DeterminePlayerStatus()
    {
        CheckTurn();
        CheckGround();
        CheckSlope();
        CheckFall();
    }


    private void Move_Performed(InputAction.CallbackContext ctx)
    {
        _moveAxisValue = ctx.ReadValue<Vector2>();
        if (_moveAxisValue.x != 0) _isRunning = true;
    }

    private void Move_Canceled(InputAction.CallbackContext ctx)
    {
        _moveAxisValue = Vector2.zero;
        _isRunning = false;

        _rb2D.velocity = new Vector2(0, _rb2D.velocity.y);
    }

    private void Jump_Started(InputAction.CallbackContext ctx)
    {
        _isJumping = true;
        _jumpStartTime = Time.time;
    }

    private void Jump_Performed(InputAction.CallbackContext ctx)
    {
        _isJumping = false;
    }

    private void Jump_Canceled(InputAction.CallbackContext ctx)
    {
        //if (_isJumping) _rb2D.velocity = new Vector2(_rb2D.velocity.x, 0);
        _isJumping = false;
    }

    private void UseItem_Performed(InputAction.CallbackContext ctx, int index)
    {
        Debug.Log("使用道具" + index);
    }

    private void SwitchItem_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("切换道具");
    }

    private void SwitchBullet_Performed(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        if (value > 0)
        {
            Debug.Log("上切换子弹");
        }
        else if (value < 0)
        {
            Debug.Log("下切换子弹");
        }
    }

    private void SwitchMeleeWeapon_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("切换近战武器");
    }

    private void SwitchMoveMode_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("切换移动模式");
    }

    private void AimStart_Performed(InputAction.CallbackContext ctx)
    {
        _isAiming = true;
        _crosshair.SetActive(true);
        Debug.Log("开始瞄准");
    }

    private void AimStart_Canceled(InputAction.CallbackContext ctx)
    {
        _isAiming = false;
        _crosshair.SetActive(false);
    }

    private void AimAxis_Performed(InputAction.CallbackContext ctx)
    {
        if (!_isAiming) return;
        //Debug.Log("调整瞄准位置");
        //float radius = 1.5f;
        //Vector2 pos = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
        //Debug.Log(pos);
        //Vector2 dir = (pos - (Vector2)transform.position).normalized;
        ////float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        ////angle = angle < 0 ? angle + 360 : angle;
        //_crosshair.position = (Vector2)transform.position + dir * radius;
        //_crosshair.SetActive(true);
    }

    private void Dash_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("冲刺");
    }

    private void Attack_Shoot_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("远程攻击");
    }

    private void Attack_Melee_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("近战攻击");
    }

    private void CrouchOrRoll_Performed(InputAction.CallbackContext ctx)
    {
        if (_moveAxisValue.x == 0)
        {
            Debug.Log("蹲");
        }
        else
        {
            Debug.Log("翻滚");
        }

    }

    private void PauseMenu_Performed(InputAction.CallbackContext ctx)
    {
        Debug.Log("玩家界面PauseMenu");
        GetComponent<InputManager>().SwitchActionMap(InputManager.MapType.UI);
    }

    private Vector2 CalcRunSpeed()
    {
        if (_isOnSlope)
        {
            return (_slopeDir * _runSpeed * -_moveAxisValue.x);
        }
        else
        {
            return (Vector2.right * _runSpeed * _moveAxisValue.x);
        }
    }

    private Vector2 CalcJumpSpeed()
    {
        float verSpeed;
        //在已知每次跳跃最大高度、每次跳跃最大时间、到达最大高度速度为零的情况下，计算各个时刻的瞬时速度
        verSpeed = Mathf.Max(0, (2 * _maxJumpHeight / _maxJumpTime) * (1 - (Time.time - _jumpStartTime) / _maxJumpTime));
        return (Vector2.up * verSpeed);
    }

    private Vector2 CalcFallSpeed()
    {
        float speed;
        speed = Mathf.Max(_maxFallSpeed, _fallStartYSpeed + _maxFallSpeed / _maxFallSpeedTime * (Time.time - _fallStartTime));
        return Vector2.up * speed;
    }

    private void CheckGround()
    {
        _isGrounded = DetectObstacle((Vector2)transform.position + _footOffset_L, Vector2.down, 0.1f, "Ground") ||
                      DetectObstacle((Vector2)transform.position + _footOffset_R, Vector2.down, 0.1f, "Ground");
        //Debug.DrawRay((Vector2)transform.position + _footOffset_L, Vector2.down);
        //Debug.DrawRay((Vector2)transform.position + _footOffset_R, Vector2.down);
    }

    private void CheckSlope()
    {
        if (!_isGrounded) return;

        float angle;
        //Vector2 perpDir;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + _footOffset_M, Vector2.down, 5.0f, LayerMask.GetMask("Ground"));
        if (hit)
        {
            _slopeDir = Vector2.Perpendicular(hit.normal);
            //Debug.DrawRay(transform.position, _slopeDir * -transform.localScale.x, Color.green);
            //Debug.Log(Vector2.Angle(moveDir, Vector2.up));
        }
        else
        {
            _isOnSlope = false;
            return;
        }

        angle = Vector2.Angle(Vector2.up, _slopeDir);
        //Debug.Log(angle);
        if (angle != 90.0f) _isOnSlope = true;
    }

    private void CheckFall()
    {
        if ((!_isGrounded && !_isJumping) != _isFalling)
        {
            _isFalling = !_isFalling;

            //如果转变成下落状态，更新下落参数
            if (_isFalling)
            {
                _fallStartTime = Time.time;
                _fallStartYSpeed = _rb2D.velocity.y;
            }
        }
    }

    private void CheckTurn()
    {
        //X输入不为0且方向与之前的不一致，则触发转向
        if ((int)_moveAxisValue.x != 0 && (_moveAxisValue != _lastMoveAxisValue))
        {
            _lastMoveAxisValue = _moveAxisValue;
            transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            //_isTurnning = true;
        }
    }

    private bool DetectObstacle(Vector2 pos, Vector2 dir, float dis, string layerName)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, dis, LayerMask.GetMask(layerName));
        return hit;
    }
}
