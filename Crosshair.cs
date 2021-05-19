using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    private Transform _player;
    private Mouse _mouse;

    private void Awake()
    {
        _mouse = Mouse.current;
        _player = GameObject.FindGameObjectWithTag("Player").transform.Find("Player-Reversible/PlayerBody").transform;
    }

    private void LateUpdate()
    {
        Follow();
    }

    private void Follow()
    {
        float radius = 1.5f;
        Vector2 pos = Camera.main.ScreenToWorldPoint(_mouse.position.ReadValue());
        Vector2 dir = (pos - (Vector2)_player.position).normalized;
        transform.position = (Vector2)_player.position + dir * radius;
    }
}
