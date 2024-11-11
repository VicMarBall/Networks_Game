using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class NetComponent : MonoBehaviour
{
	public NetObject netObject;

	private void Awake()
	{
		if (netObject == null && GetComponent<NetObject>() != null) 
		{
			netObject = GetComponent<NetObject>();
		}
	}
}
