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

	struct NetObjectDataToInstantiate
	{
		public int userOwnerID;
		public int netID;
		public GameObject netObjectPrefab;
		public Vector3 position;
	}

	Queue<NetObjectDataToInstantiate> netObjectsPendingToInstantiate = new Queue<NetObjectDataToInstantiate>();

	Dictionary<int, byte[]> updateDataReceived = new Dictionary<int, byte[]>();
	Dictionary<int, byte[]> updateDataToSend = new Dictionary<int, byte[]>();

	float timeSinceLastSending;

	Queue<ObjectStateSegment> preparedObjectStateSegmentsToSend = new Queue<ObjectStateSegment>();

	#region NETOBJECT PREFAB LIBRARY
	[Header("Prefab Library")]
	[SerializeField] GameObject localPlayerPrefab;
	[SerializeField] GameObject foreignPlayerPrefab;
	#endregion

	private void Update()
	{
		timeSinceLastSending += Time.deltaTime;

		while (netObjectsPendingToInstantiate.Count > 0)
		{
			NetObjectDataToInstantiate netObjectToCreate = netObjectsPendingToInstantiate.Dequeue();

			if (!netObjects.ContainsKey(netObjectToCreate.netID))
			{
				GameObject GO = Instantiate(netObjectToCreate.netObjectPrefab.gameObject, netObjectToCreate.position, Quaternion.identity);
				NetObject netObject = GO.GetComponent<NetObject>();
				if (netObject == null)
				{
					netObject = GO.GetComponentInChildren<NetObject>();
				}
				netObject.isOwner = (netObjectToCreate.userOwnerID == NetworkingEnd.instance.userID);
				netObjects.Add(netObjectToCreate.netID, netObject);
			}
		}

		if (timeSinceLastSending > 0.1)
		{
			timeSinceLastSending = 0;

			foreach (var toSend in updateDataToSend)
			{
				ObjectStateSegment objectStateSegment = new ObjectStateSegment(ObjectReplicationAction.UPDATE, toSend.Key, netObjects[toSend.Key].type, toSend.Value);
				preparedObjectStateSegmentsToSend.Enqueue(objectStateSegment);
			}

			if (preparedObjectStateSegmentsToSend.Count > 0)
				SendObjectStatePacket();
		}

		foreach (var item in updateDataReceived)
		{
			netObjects[item.Key].ReceiveData(item.Value);
		}
		updateDataReceived.Clear();

	}

	private void LateUpdate()
	{
		//if (preparedObjectStateSegmentsToSend.Count > 0)
		//{
		//	SendObjectStatePacket();
		//}
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

		NetworkingEnd.instance.SendPacketToAllUsers(packet);
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
			case NetObjectClass.PLAYER:
				NetObjectDataToInstantiate playerData = new NetObjectDataToInstantiate();
				playerData.netID = netObjects.Count;
				
				Stream stream = new MemoryStream(data);
				BinaryReader reader = new BinaryReader(stream);
				stream.Seek(0, SeekOrigin.Begin);

				playerData.userOwnerID = reader.ReadInt32();

				if (playerData.userOwnerID == NetworkingEnd.instance.userID)
				{
					playerData.netObjectPrefab = localPlayerPrefab;
				}
				else
				{
					playerData.netObjectPrefab = foreignPlayerPrefab;
				}


				playerData.position.x = reader.ReadSingle();
				playerData.position.y = reader.ReadSingle();
				playerData.position.z = reader.ReadSingle();

				stream.Close();
				
				netObjectsPendingToInstantiate.Enqueue(playerData);

				preparedObjectStateSegmentsToSend.Enqueue(new ObjectStateSegment(ObjectReplicationAction.RECREATE, playerData.netID, NetObjectClass.PLAYER, data));
				break;
		}
	}

	void RecreateNetObject(int netID, NetObjectClass classToReplicate, byte[] data)
	{
		switch (classToReplicate)
		{
			case NetObjectClass.PLAYER:
				NetObjectDataToInstantiate playerData = new NetObjectDataToInstantiate();
				playerData.netID = netID;

				Stream stream = new MemoryStream(data);
				BinaryReader reader = new BinaryReader(stream);
				stream.Seek(0, SeekOrigin.Begin);

				playerData.userOwnerID = reader.ReadInt32();

				if (playerData.userOwnerID == NetworkingEnd.instance.userID)
				{
					playerData.netObjectPrefab = localPlayerPrefab;
				}
				else
				{
					playerData.netObjectPrefab = foreignPlayerPrefab;
				}

				playerData.position.x = reader.ReadSingle();
				playerData.position.y = reader.ReadSingle();
				playerData.position.z = reader.ReadSingle();

				stream.Close();

				netObjectsPendingToInstantiate.Enqueue(playerData);
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

	// TO IMPLEMENT
	void DestroyNetObject(int netID)
	{

	}

	public void PrepareNetObjectCreate(NetObjectClass classToCreate, byte[] data)
	{
		CreateNetObject(classToCreate, data);
	}

	public void PrepareNetObjectUpdate(int netID, byte[] data)
	{
		if (updateDataToSend.ContainsKey(netID))
			updateDataToSend[netID] = data;
		else
			updateDataToSend.Add(netID, data);
	}
}
