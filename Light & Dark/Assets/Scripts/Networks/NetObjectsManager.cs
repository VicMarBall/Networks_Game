using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectStatePacketBody;

public class NetObjectsManager : MonoBehaviour
{
	// singleton
	public static NetObjectsManager instance { get; private set; }

	Dictionary<int, GameObject> networkObjects = new Dictionary<int, GameObject>();

	[SerializeField] GameObject localPlayerPrefab;
	[SerializeField] GameObject foreignPlayerPrefab;

	Queue<ObjectStatePacketBodySegment> preparedObjectStateSegments = new Queue<ObjectStatePacketBodySegment>();

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
			int netID = netIDPendingToCreate.Dequeue();
			networkObjects.Add(netID, go);
			go.GetComponent<NetObject>().netID = netID;
		}
	}

	private void LateUpdate()
	{
		if (preparedObjectStateSegments.Count > 0)
		{
			SendObjectStatePacket();
		}
	}

	public void PrepareBodySegment(ObjectStatePacketBodySegment packetBody)
	{
		preparedObjectStateSegments.Enqueue(packetBody);
	}

	void SendObjectStatePacket()
	{
		int MTU = 1000;

		ObjectStatePacketBody packetBody = new ObjectStatePacketBody();
		
		int packetSize = 0;
		while (preparedObjectStateSegments.Count > 0)
		{
			ObjectStatePacketBodySegment segmentToAdd = preparedObjectStateSegments.Dequeue();
			packetSize += segmentToAdd.objectData.Length + 96;
			if (packetSize > MTU) 
			{	
				preparedObjectStateSegments.Enqueue(segmentToAdd);
				break;
			}

			packetBody.AddSegment(segmentToAdd);
		}

		// TO CHANGE playerID
		Packet packet = new Packet(PacketType.OBJECT_STATE, NetworkingEnd.instance.userID, packetBody);

		NetworkingEnd.instance.PreparePacket(packet);
	}

	public void ManageObjectStatePacket(ObjectStatePacketBody packetBody)
	{
		foreach (ObjectStatePacketBodySegment segment in packetBody.segments)
		{
			switch (segment.action)
			{
				case ObjectReplicationAction.CREATE:
					CreateNetObject(segment.objectClass, segment.objectData);
					break;
				case ObjectReplicationAction.RECREATE:
					RecreateNetObject(segment.netID, segment.objectClass, segment.objectData);
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

	void CreateNetObject(ObjectReplicationClass classToReplicate, byte[] data)
	{
		switch (classToReplicate)
		{
			case ObjectReplicationClass.LOCAL_PLAYER:
				netIDPendingToCreate.Enqueue(networkObjects.Count + netIDPendingToCreate.Count);
				objectsPendingToCreate.Enqueue(localPlayerPrefab);
				break;
			case ObjectReplicationClass.FOREIGN_PLAYER:
				int newNetID = networkObjects.Count + netIDPendingToCreate.Count;

				netIDPendingToCreate.Enqueue(newNetID);
				objectsPendingToCreate.Enqueue(foreignPlayerPrefab);

				ObjectStatePacketBody body = new ObjectStatePacketBody();
				body.AddSegment(ObjectReplicationAction.RECREATE, newNetID, ObjectReplicationClass.LOCAL_PLAYER, data);

				NetworkingEnd.instance.PreparePacket(new Packet(PacketType.OBJECT_STATE, NetworkingEnd.instance.userID, body));
				break;
		}
	}

	void RecreateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
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
	}

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

	public void CreateLocalPlayer()
	{
		netIDPendingToCreate.Enqueue(networkObjects.Count + netIDPendingToCreate.Count);
		objectsPendingToCreate.Enqueue(localPlayerPrefab);
	}
}
