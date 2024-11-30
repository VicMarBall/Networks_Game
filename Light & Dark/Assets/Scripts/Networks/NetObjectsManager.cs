using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

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

	[SerializeField] NetObject localPlayerPrefab;
	[SerializeField] NetObject foreignPlayerPrefab;

	Queue<ObjectStateSegment> preparedObjectStateSegmentsToSend = new Queue<ObjectStateSegment>();

	struct NetObjectToCreateData
	{
		public int netID;
		public NetObject netObjectPrefab;
		public Vector3 position;
	}

	Queue<NetObjectToCreateData> netObjectsPendingToInstantiate = new Queue<NetObjectToCreateData>();

	private void Update()
	{
		while (netObjectsPendingToInstantiate.Count > 0)
		{
			NetObjectToCreateData netObjectToCreate = netObjectsPendingToInstantiate.Dequeue();

			if (!netObjects.ContainsKey(netObjectToCreate.netID))
			{
				GameObject GO = Instantiate(netObjectToCreate.netObjectPrefab.gameObject);
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

	public void PrepareBodySegment(ObjectStateSegment packetBody)
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
			ObjectStateSegment segmentToAdd = preparedObjectStateSegmentsToSend.Dequeue();
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
		foreach (ObjectStateSegment segment in packetBody.segments)
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
					PrepareUpdateNetObject(segment.netID, segment.objectData);
					break;
				case ObjectReplicationAction.DESTROY:
					DestroyNetObject(segment.netID);
					break;
			}
		}
	}

	void CreateNetObject(NetObjectClass classToReplicate, byte[] data)
	{
		switch (classToReplicate)
		{
			case NetObjectClass.LOCAL_PLAYER:
				NetObjectToCreateData localPlayerData = new NetObjectToCreateData();
				localPlayerData.netID = netObjects.Count;
				localPlayerData.netObjectPrefab = localPlayerPrefab;
				{
					Stream stream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(stream);
					stream.Seek(0, SeekOrigin.Begin);

					localPlayerData.position.x = reader.ReadSingle();
					localPlayerData.position.y = reader.ReadSingle();
					localPlayerData.position.z = reader.ReadSingle();

					stream.Close();
				}
				netObjectsPendingToInstantiate.Enqueue(localPlayerData);
				break;
			case NetObjectClass.FOREIGN_PLAYER:
				NetObjectToCreateData foreignPlayerData = new NetObjectToCreateData();
				foreignPlayerData.netID = netObjects.Count;
				foreignPlayerData.netObjectPrefab = localPlayerPrefab;
				{
					Stream stream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(stream);
					stream.Seek(0, SeekOrigin.Begin);

					foreignPlayerData.position.x = reader.ReadSingle();
					foreignPlayerData.position.y = reader.ReadSingle();
					foreignPlayerData.position.z = reader.ReadSingle();

					stream.Close();
				}
				netObjectsPendingToInstantiate.Enqueue(foreignPlayerData);

				break;
		}
	}

	void RecreateNetObject(int netID, NetObjectClass classToReplicate, byte[] data)
	{
		switch (classToReplicate)
		{
			case NetObjectClass.LOCAL_PLAYER:
				NetObjectToCreateData localPlayerData = new NetObjectToCreateData();
				localPlayerData.netID = netID;
				localPlayerData.netObjectPrefab = localPlayerPrefab;
				{
					Stream stream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(stream);
					stream.Seek(0, SeekOrigin.Begin);

					localPlayerData.position.x = reader.ReadSingle();
					localPlayerData.position.y = reader.ReadSingle();
					localPlayerData.position.z = reader.ReadSingle();

					stream.Close();
				}
				netObjectsPendingToInstantiate.Enqueue(localPlayerData);
				break;
			case NetObjectClass.FOREIGN_PLAYER:
				NetObjectToCreateData foreignPlayerData = new NetObjectToCreateData();
				foreignPlayerData.netID = netID;
				foreignPlayerData.netObjectPrefab = foreignPlayerPrefab;
				{
					Stream stream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(stream);
					stream.Seek(0, SeekOrigin.Begin);

					foreignPlayerData.position.x = reader.ReadSingle();
					foreignPlayerData.position.y = reader.ReadSingle();
					foreignPlayerData.position.z = reader.ReadSingle();

					stream.Close();
				}
				netObjectsPendingToInstantiate.Enqueue(foreignPlayerData);
				break;
		}
	}

	void PrepareUpdateNetObject(int netID, byte[] data)
	{
		if (updateDataReceived.ContainsKey(netID))
		{
			updateDataReceived[netID] = data;
		}
		else
		{
			updateDataReceived.Add(netID, data);
		}
	}

	void UpdateNetObject(int netID, byte[] data)
	{
		netObjects[netID].ReceiveData(data);
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
