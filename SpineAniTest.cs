using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEngine.InputSystem;

public class SpineAniTest : MonoBehaviour
{
    private SkeletonAnimation _skeletonAnimation;
    private PlayerInput _playerInput;
    private Coroutine _attackCor;
    private string[] _attackAniName = { "gongji", "tiaodong2", "zhuangji", "gundong", "chongfeng" };
    private float _chargeStartTime;
    private int _attackCode;
    private int _lastAttackCode;
    private int _chargeAttackPhase;
    private bool _isCharging;

    private void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        //_playerInput.actions["Attack_Melee"].started += Attack_Started;
        _playerInput.actions["Attack_Melee"].performed += Attack_Performed;
        _playerInput.actions["Attack_Melee"].canceled += Attack_Canceled;
    }

    void Start()
    {
        Debug.Log("777");
    }

    // Update is called once per frame
    void Update()
    {
        RefreshPlayerAnimation();
        if (_isCharging && Time.time - _chargeStartTime >= 1.5f && _chargeAttackPhase < 2)
        {
            //Debug.Log("Chanrge Complete");
            _chargeAttackPhase++;
            _chargeStartTime = Time.time;
        }
        //Debug.Log(_chargeAttackPhase);
    }

    private void Attack_Performed(InputAction.CallbackContext ctx)
    {
        _isCharging = true;
        _chargeStartTime = Time.time;
        TrackEntry te = _skeletonAnimation.state.GetCurrent(0);
        if (_attackCode != 0)
        {
            if ((te.AnimationTime / te.AnimationEnd) > 0.7f && te.Animation.Name == _attackAniName[_attackCode - 1])
            {
                _attackCode %= 3;
                _attackCode++;
            }
        }
        else
        {
            _attackCode = 1;
        }
    }

    private void Attack_Canceled(InputAction.CallbackContext ctx)
    {
        _isCharging = false;
        switch (_chargeAttackPhase)
        {
            case 1:_attackCode = 4;break;
            case 2:_attackCode = 5;break;
            default:break;
        }
        _chargeAttackPhase = 0;
    }

    private void RefreshPlayerAnimation()
    {
        if (_attackCode != 0 && _lastAttackCode != _attackCode)
        {
            TrackEntry te;
            if (_lastAttackCode == 0 && _attackCode == 1)//&& !_attackAniName.Exists(x => x == _skeletonAnimation.state.GetCurrent(0).Animation.Name))
            {
                te = _skeletonAnimation.state.SetAnimation(0, _attackAniName[0], false);
            }
            else
            {
                te = _skeletonAnimation.state.AddAnimation(0, _attackAniName[_attackCode - 1], false, 0);
            }
            _lastAttackCode = _attackCode;
            te.Start += _ => StopAttackCor();
            te.Complete += _ => OnAttackAniComplete();
        }
    }


    private void OnAttackAniComplete()
    {
        _attackCor = StartCoroutine(IEResetAttackCodeAfterScenonds());
    }

    private void StopAttackCor()
    {
        //Debug.Log("Stop Coroutine Start");
        if (_attackCor != null) 
        {
            StopCoroutine(_attackCor); 
            _attackCor = null;
            //Debug.Log("Stop"); 
        }
    }

    IEnumerator IEResetAttackCodeAfterScenonds(float seconds = 0.5f)
    {
        //Debug.Log("Coroutine Start");
        yield return new WaitForSeconds(seconds);
        _attackCode = 0;
        _lastAttackCode = 0;
        _attackCor = null;
        //Debug.Log("Coroutine Finish");
    }
}
