using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 布尔事件
/// </summary>
[System.Serializable]
public class BoolEvent : UnityEvent<bool, GameObject>
{

} 
/// <summary>
/// 范围触发器
/// </summary>
public class ZoneTriggerController : MonoBehaviour
{
	/// <summary>
	/// 进入范围事件
	/// </summary>
	[SerializeField] private BoolEvent _enterZone = default;
	/// <summary>
	/// 层遮罩
	/// </summary>
	[SerializeField] private LayerMask _layers = default;


  
    private void OnTriggerEnter2D(Collider2D other)
	{
		if ((1 << other.gameObject.layer & _layers) != 0)
		{
			_enterZone.Invoke(true, other.gameObject);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((1 << other.gameObject.layer & _layers) != 0)
		{
			_enterZone.Invoke(false, other.gameObject);
		}
	}
}
