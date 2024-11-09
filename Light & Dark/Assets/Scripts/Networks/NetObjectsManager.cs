using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectStatePacketBody;

public class NetObjectsManager : MonoBehaviour
{
	// singleton
	public static NetObjectsManager instance { get; private set; }

	Dictionary<int, GameObject> networkObjects;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void TestManager() 
	{
		Debug.Log("TestManager Reached");
	}

	public void UpdateNetworkObjects(ObjectStatePacketBody packetBody)
	{
		foreach (ObjectStatePacketBodySegment segment in packetBody.segments)
		{
			switch (segment.action)
			{
				case ObjectReplicationAction.CREATE:
					CreateNetObject(segment.networkObjectID, segment.objectClass, segment.data);
					break;
				case ObjectReplicationAction.UPDATE:
					UpdateNetObject(segment.networkObjectID, segment.objectClass, segment.data);
					break;
				case ObjectReplicationAction.DESTROY:
					DestroyNetObject(segment.networkObjectID);
					break;
			}
		}
	}

	// TO IMPLEMENT
	void CreateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
		GameObject go = null;


		networkObjects.Add(netID, go);
	}

	// TO IMPLEMENT
	void UpdateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{

	}

	// TO IMPLEMENT
	void DestroyNetObject(int netID)
	{

	}
}
