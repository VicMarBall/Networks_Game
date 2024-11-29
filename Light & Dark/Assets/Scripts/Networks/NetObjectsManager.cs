using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectsManager : MonoBehaviour
{
	#region SINGLETON
	public static NetObjectsManager instance { get; private set; }

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

	#endregion

	Dictionary<int, NetObject> netObjects = new Dictionary<int, NetObject>();

	Dictionary<int, byte[]> updateDataReceived = new Dictionary<int, byte[]>();
	Dictionary<int, byte[]> updateDataToSend = new Dictionary<int, byte[]>();

	[SerializeField] GameObject localPlayerPrefab;
	[SerializeField] GameObject foreignPlayerPrefab;

	Queue<ObjectStatePacketBodySegment> preparedObjectStateSegmentsToSend = new Queue<ObjectStatePacketBodySegment>();

	struct NetObjectData
	{
		public int netID;
		public NetObject netObject;
	}

	Queue<NetObjectData> netObjectsPendingToCreate = new Queue<NetObjectData>();

	private void Update()
	{
		while (netObjectsPendingToCreate.Count > 0)
		{
			NetObjectData netObjectToCreate = netObjectsPendingToCreate.Dequeue();

			if (!netObjects.ContainsKey(netObjectToCreate.netID))
			{
				GameObject GO = Instantiate(netObjectToCreate.netObject.gameObject);
				NetObject netObject = GO.GetComponent<NetObject>();
				netObjects.Add(netObjectToCreate.netID, netObject);
			}
		}

		foreach (var item in updateDataReceived)
		{
			netObjects[item.Key].ReceiveData(item.Value);
		}
		updateDataReceived.Clear();
	}

	private void LateUpdate()
	{
		if (preparedObjectStateSegmentsToSend.Count > 0)
		{
			SendObjectStatePacket();
		}
	}

	public void PrepareBodySegment(ObjectStatePacketBodySegment packetBody)
	{
		preparedObjectStateSegmentsToSend.Enqueue(packetBody);
	}

	void SendObjectStatePacket()
	{
		int MTU = 1000;

		ObjectStatePacketBody packetBody = new ObjectStatePacketBody();
		
		int packetSize = 0;
		while (preparedObjectStateSegmentsToSend.Count > 0)
		{
			ObjectStatePacketBodySegment segmentToAdd = preparedObjectStateSegmentsToSend.Dequeue();
			packetSize += segmentToAdd.objectData.Length + 96;
			if (packetSize > MTU) 
			{	
				preparedObjectStateSegmentsToSend.Enqueue(segmentToAdd);
				break;
			}

			packetBody.AddSegment(segmentToAdd);
		}

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
					PrepareUpdateNetObject(segment.netID, segment.objectClass, segment.objectData);
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
				netIDPendingToCreate.Enqueue(netObjects.Count + netIDPendingToCreate.Count);
				objectsPendingToCreate.Enqueue(localPlayerPrefab);
				break;
			case ObjectReplicationClass.FOREIGN_PLAYER:
				int newNetID = netObjects.Count + netIDPendingToCreate.Count;

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

	void PrepareUpdateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
		UpdateObjectInfo updateObjectInfo = new UpdateObjectInfo();
		updateObjectInfo.netID = netID;
		updateObjectInfo.classToReplicate = classToReplicate;
		updateObjectInfo.data = data;
		objectsPendingToUpdate.Enqueue(updateObjectInfo);
	}

	void UpdateNetObject(int netID, ObjectReplicationClass classToReplicate, byte[] data)
	{
		switch (classToReplicate)
		{
			case ObjectReplicationClass.POSITION:
				Vector3 position = ObjectReplicationRegistry.DeserializeVector3(data);
				netObjects[netID].transform.position = position;
				break;
			case ObjectReplicationClass.ROTATION:
				Quaternion rotation = ObjectReplicationRegistry.DeserializeQuaternion(data);
				netObjects[netID].transform.rotation = rotation;
				break;
			case ObjectReplicationClass.SCALE:
				Vector3 scale = ObjectReplicationRegistry.DeserializeVector3(data);
				netObjects[netID].transform.localScale = scale;
				break;
		}
	}

	// TO IMPLEMENT
	void DestroyNetObject(int netID)
	{

	}

	//public void CreateLocalPlayer()
	//{
	//	netIDPendingToCreate.Enqueue(netObjects.Count + netIDPendingToCreate.Count);
	//	objectsPendingToCreate.Enqueue(localPlayerPrefab);
	//}

	public void ReceiveNetObjectData(int netID, byte[] data)
	{
		if (updateDataToSend.ContainsKey(netID))
			updateDataToSend[netID] = data;
		else
			updateDataToSend.Add(netID, data);
	}
}
