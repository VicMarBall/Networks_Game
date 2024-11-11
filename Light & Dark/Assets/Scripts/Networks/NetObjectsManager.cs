using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectStatePacketBody;

public class NetObjectsManager : MonoBehaviour
{
	// singleton
	public static NetObjectsManager instance { get; private set; }

	[SerializeField] GameObject localPlayerPrefab;
	[SerializeField] GameObject foreignPlayerPrefab;

	Dictionary<int, GameObject> networkObjects = new Dictionary<int, GameObject>();

	Queue<ObjectStatePacketBodySegment> preparedPacketBodies = new Queue<ObjectStatePacketBodySegment>();

	Queue<int> netIDPendingToCreate = new Queue<int>();
	Queue<GameObject> objectsPendingToCreate = new Queue<GameObject>();

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

	private void Update()
	{
		while (objectsPendingToCreate.Count > 0)
		{
			GameObject go = Instantiate(objectsPendingToCreate.Dequeue());
			networkObjects.Add(netIDPendingToCreate.Dequeue(), go);
		}
	}

	private void LateUpdate()
	{
		if (preparedPacketBodies.Count > 0)
		{
			SendObjectStatePacket();
		}
	}

	public void PreparePacket(ObjectStatePacketBodySegment packetBody)
	{
		preparedPacketBodies.Enqueue(packetBody);
	}

	void SendObjectStatePacket()
	{
		int MTU = 1000;

		ObjectStatePacketBody packetBody = new ObjectStatePacketBody();
		
		int packetSize = 0;
		while (preparedPacketBodies.Count > 0)
		{
			ObjectStatePacketBodySegment segmentToAdd = preparedPacketBodies.Dequeue();
			packetSize += segmentToAdd.objectData.Length + 96;
			if (packetSize > MTU) 
			{	
				preparedPacketBodies.Enqueue(segmentToAdd);
				break;
			}

			packetBody.AddSegment(segmentToAdd);
		}

		// TO CHANGE playerID
		Packet packet = new Packet(PacketType.OBJECT_STATE, 0, packetBody);

		NetworkingEnd.instance.PreparePacket(packet);
	}

	public void ManageObjectStatePacket(ObjectStatePacketBody packetBody)
	{
		foreach (ObjectStatePacketBodySegment segment in packetBody.segments)
		{
			switch (segment.action)
			{
				case ObjectReplicationAction.CREATE:
					CreateNetObject(segment.netID, segment.objectClass, segment.objectData);
					break;
				case ObjectReplicationAction.UPDATE:
					UpdateNetObject(segment.netID, segment.objectClass, segment.objectData);
					break;
				case ObjectReplicationAction.DESTROY:
					DestroyNetObject(segment.netID);
					break;
			}
		}
	}

	public void CreateLocalPlayer()
	{
		CreateNetObject(networkObjects.Count, ObjectReplicationClass.LOCAL_PLAYER, new byte[1]);

		ObjectStatePacketBodySegment playerSegment = new ObjectStatePacketBodySegment(ObjectReplicationAction.CREATE, networkObjects.Count, ObjectReplicationClass.LOCAL_PLAYER, new byte[1]);
		PreparePacket(playerSegment);
	}

	public void CreateForeignPlayer()
	{
		CreateNetObject(networkObjects.Count, ObjectReplicationClass.FOREIGN_PLAYER, new byte[1]);
	}

	// TO IMPLEMENT
	void CreateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
		GameObject go = null;

		switch (classToReplicate)
		{
			case ObjectReplicationClass.LOCAL_PLAYER:
				netIDPendingToCreate.Enqueue(netID);
				objectsPendingToCreate.Enqueue(localPlayerPrefab);
				break;
			case ObjectReplicationClass.FOREIGN_PLAYER:
				netIDPendingToCreate.Enqueue(netID);
				objectsPendingToCreate.Enqueue(foreignPlayerPrefab);
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
