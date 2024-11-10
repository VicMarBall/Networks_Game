using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetObject))]
abstract public class NetComponent : MonoBehaviour
{
	protected NetObject netObject;

	private void Awake()
	{
		netObject = GetComponent<NetObject>();
	}
}
