using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetObjectType
{
	NONE,
	PLAYER,
	BOX
}

public abstract class NetObject : MonoBehaviour
{
	public bool isOwner;
	public int netID;
	public NetObjectType type { get; protected set; }

	public abstract void ReceiveData(byte[] data);
}