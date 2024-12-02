using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetObject : MonoBehaviour
{
	public int ownerID;
	public int netID;
	public NetObjectClass type { get; protected set; }

	public abstract void ReceiveUpdateData(byte[] data);
	public abstract byte[] SerializeToRecreate();

	public bool IsOwner() { return (ownerID == NetworkingEnd.instance.userID); }
}