using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectStatePacketBody;

public class NetObjectsManager : MonoBehaviour
{
	// singleton
	public static NetObjectsManager instance { get; private set; }

	[SerializeField]
	GameObject playerPrefab;

	Dictionary<int, GameObject> networkObjects;

	List<ObjectStatePacketBodySegment> preparedPacketBodies = new List<ObjectStatePacketBodySegment>();

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

	private void LateUpdate()
	{
		if (preparedPacketBodies.Count > 0)
		{
			SendObjectStatePacket();
		}
	}

	public void TestManager() 
	{
		Debug.Log("TestManager Reached");
	}

	public void PreparePacket(ObjectStatePacketBodySegment packetBody)
	{
		preparedPacketBodies.Add(packetBody);
	}

	void SendObjectStatePacket()
	{
		int MTU = 1000;

		ObjectStatePacketBody packetBody = new ObjectStatePacketBody();
		
		int packetSize = 0;
		while (preparedPacketBodies.Count > 0)
		{
			packetSize += preparedPacketBodies[preparedPacketBodies.Count - 1].data.Length + 96;
			if (packetSize > MTU) {	break; }

			packetBody.AddSegment(preparedPacketBodies[preparedPacketBodies.Count - 1]);
			preparedPacketBodies.RemoveAt(preparedPacketBodies.Count - 1);
		}

		// TO CHANGE playerID
		Packet packet = new Packet(PacketBodyType.OBJECT_STATE, 0, packetBody);

		NetworkingEnd.instance.PreparePacket(packet);
	}

	public void ManageObjectStatePacket(ObjectStatePacketBody packetBody)
	{
		foreach (ObjectStatePacketBodySegment segment in packetBody.segments)
		{
			switch (segment.action)
			{
				case ObjectReplicationAction.CREATE:
					CreateNetObject(segment.netID, segment.objectClass, segment.data);
					break;
				case ObjectReplicationAction.UPDATE:
					UpdateNetObject(segment.netID, segment.objectClass, segment.data);
					break;
				case ObjectReplicationAction.DESTROY:
					DestroyNetObject(segment.netID);
					break;
			}
		}
	}

	public void CreatePlayer()
	{
		CreateNetObject(networkObjects.Count, ObjectReplicationClass.PLAYER, new byte[1]);

		ObjectStatePacketBodySegment playerSegment = new ObjectStatePacketBodySegment(ObjectReplicationAction.CREATE, networkObjects.Count, ObjectReplicationClass.PLAYER, new byte[1]);
		PreparePacket(playerSegment);
	}

	// TO IMPLEMENT
	void CreateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
		GameObject go = null;

		switch (classToReplicate)
		{
			case ObjectReplicationClass.PLAYER:
				go = Instantiate(playerPrefab);
				go.GetComponent<PlayerMovement>().getsInputs = true;
			break;
		}

		if (go != null)
		{
			networkObjects.Add(netID, go);
		}
	}

	// TO IMPLEMENT
	void UpdateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
		switch (classToReplicate)
		{
			case ObjectReplicationClass.POSITION:
				Vector3 position = ObjectReplicationRegistry.DeserializeVector3(data);
				networkObjects[netID].transform.position = position;
				break;
			case ObjectReplicationClass.ROTATION:
				Quaternion rotation = ObjectReplicationRegistry.DeserializeQuaternion(data);
				networkObjects[netID].transform.rotation = rotation;
				break;
			case ObjectReplicationClass.SCALE:
				Vector3 scale = ObjectReplicationRegistry.DeserializeVector3(data);
				networkObjects[netID].transform.localScale = scale;
				break;
		}
	}

	// TO IMPLEMENT
	void DestroyNetObject(int netID)
	{

	}
}
