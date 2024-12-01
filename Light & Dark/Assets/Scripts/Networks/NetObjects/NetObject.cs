using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetObject : MonoBehaviour
{
	public bool isOwner;
	public int netID;
	public NetObjectClass type { get; protected set; }

	public abstract void ReceiveUpdateData(byte[] data);
	public abstract byte[] SerializeToRecreate();
}