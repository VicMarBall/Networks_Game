using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

	int nextNetID = 0;

	Queue<DataToCreateNetObject> pendingToCreate = new Queue<DataToCreateNetObject>();
	Queue<DataToRecreateNetObject> pendingToRecreate = new Queue<DataToRecreateNetObject>();
	Queue<DataToUpdateNetObject> pendingToUpdate = new Queue<DataToUpdateNetObject>();
	Queue<DataToDestroyNetObject> pendingToDestroy = new Queue<DataToDestroyNetObject>();

	float timeSinceLastSending;
	Dictionary<int, ObjectStateSegment> objectStatesToSend = new Dictionary<int, ObjectStateSegment>();

	#region NETOBJECT PREFAB LIBRARY
	[Header("Prefab Library")]
	[SerializeField] GameObject localPlayerPrefab;
	[SerializeField] GameObject foreignPlayerPrefab;
	#endregion

	private void Update()
	{
		timeSinceLastSending += Time.deltaTime;

		while (pendingToDestroy.Count > 0)
		{
			DataToDestroyNetObject dataToDestroy = pendingToDestroy.Dequeue();
			DestroyNetObject(dataToDestroy);
		}

		while (pendingToUpdate.Count > 0)
		{
			DataToUpdateNetObject dataToUpdate = pendingToUpdate.Dequeue();
			UpdateNetObject(dataToUpdate);
		}

		while (pendingToRecreate.Count > 0)
		{
			DataToRecreateNetObject dataToRecreate = pendingToRecreate.Dequeue();
			RecreateNetObject(dataToRecreate);
		}

		while (pendingToCreate.Count > 0)
		{
			DataToCreateNetObject dataToCreate = pendingToCreate.Dequeue();
			CreateNetObject(dataToCreate);
		}

		SendObjectStatePacket();
	}

	public void ReceiveObjectStateToSend(int netID, ObjectStateSegment segment)
	{
		if (objectStatesToSend.ContainsKey(netID))
		{
			objectStatesToSend[netID] = segment;
		}
		else
		{
			objectStatesToSend.Add(netID, segment);
		}
	}

	void SendObjectStatePacket()
	{
		//int MTU = 1000;

		ObjectStatePacketBody packetBody = new ObjectStatePacketBody();

		bool hasPacket = false;

		//int packetSize = 0;
		foreach (var objectState in objectStatesToSend.Values)
		{
			packetBody.AddSegment(objectState);
			hasPacket = true;
		}
		objectStatesToSend.Clear();
		//while (preparedObjectStateSegmentsToSend.Count > 0)
		//{
		//	ObjectStateSegment segmentToAdd = preparedObjectStateSegmentsToSend.Dequeue();
		//	packetSize += segmentToAdd.dataToAction.Length + 32;
		//	if (packetSize > MTU) 
		//	{	
		//		preparedObjectStateSegmentsToSend.Enqueue(segmentToAdd);
		//		break;
		//	}

		//	packetBody.AddSegment(segmentToAdd);
		//}
		if (!hasPacket) { return; }
		Packet packet = new Packet(PacketType.OBJECT_STATE, NetworkingEnd.instance.userID, packetBody);

		NetworkingEnd.instance.SendPacketToAllUsers(packet);
	}

	public void CreateNetObjectFromLocal(DataToCreateNetObject dataToCreate)
	{
		pendingToCreate.Enqueue(dataToCreate);
	}


	public void ManageObjectStatePacket(ObjectStatePacketBody packetBody)
	{
		foreach (ObjectStateSegment segment in packetBody.segments)
		{
			switch (segment.action)
			{
				case ObjectReplicationAction.CREATE:
					DataToCreateNetObject dataToCreate = new DataToCreateNetObject();
					dataToCreate.Deserialize(segment.dataToAction);
					pendingToCreate.Enqueue(dataToCreate);
					break;
				case ObjectReplicationAction.RECREATE:
					DataToRecreateNetObject dataToRecreate = new DataToRecreateNetObject();
					dataToRecreate.Deserialize(segment.dataToAction);
					pendingToRecreate.Enqueue(dataToRecreate);
					break;
				case ObjectReplicationAction.UPDATE:
					DataToUpdateNetObject dataToUpdate = new DataToUpdateNetObject();
					dataToUpdate.Deserialize(segment.dataToAction);
					pendingToUpdate.Enqueue(dataToUpdate);
					break;
				case ObjectReplicationAction.DESTROY:
					DataToDestroyNetObject dataToDestroy = new DataToDestroyNetObject();
					dataToDestroy.Deserialize(segment.dataToAction);
					pendingToDestroy.Enqueue(dataToDestroy);
					break;
			}
		}
	}

	void CreateNetObject(DataToCreateNetObject dataToCreate)
	{
		if (!NetworkingEnd.instance.IsServer()) { return; }
		switch (dataToCreate.netClass)
		{
			case NetObjectClass.PLAYER:

				GameObject GO;
				if (dataToCreate.ownerID == NetworkingEnd.instance.userID)
				{
					GO = Instantiate(localPlayerPrefab);
				}
				else
				{
					GO = Instantiate(foreignPlayerPrefab);
				}

				NetPlayer netPlayer = GO.GetComponent<NetPlayer>();
				if (netPlayer == null)
				{
					netPlayer = GO.GetComponentInChildren<NetPlayer>();
				}

				netPlayer.ownerID = dataToCreate.ownerID;

				netPlayer.netID = nextNetID;
				nextNetID++;

				netPlayer.UpdateObjectData(dataToCreate.objectData);

				netObjects.Add(netPlayer.netID, netPlayer);

				DataToRecreateNetObject dataToRecreate = new DataToRecreateNetObject();
				dataToRecreate.netID = netPlayer.netID;
				dataToRecreate.netClass = NetObjectClass.PLAYER;
				dataToRecreate.ownerID = dataToCreate.ownerID;
				dataToRecreate.objectData = dataToCreate.objectData;

				objectStatesToSend.Add(dataToRecreate.netID, new ObjectStateSegment(ObjectReplicationAction.RECREATE, dataToRecreate.Serialize()));

				break;
		}

	}

	void RecreateNetObject(DataToRecreateNetObject dataToRecreate)
	{
		switch (dataToRecreate.netClass)
		{
			case NetObjectClass.PLAYER:

				GameObject GO;
				if (dataToRecreate.ownerID == NetworkingEnd.instance.userID)
				{
					GO = Instantiate(localPlayerPrefab);
				}
				else
				{
					GO = Instantiate(foreignPlayerPrefab);
				}

				NetPlayer netPlayer = GO.GetComponent<NetPlayer>();
				if (netPlayer == null)
				{
					netPlayer = GO.GetComponentInChildren<NetPlayer>();
				}

				netPlayer.ownerID = dataToRecreate.ownerID;

				netPlayer.netID = dataToRecreate.netID;
				nextNetID = dataToRecreate.netID + 1;

				netPlayer.UpdateObjectData(dataToRecreate.objectData);

				netObjects.Add(netPlayer.netID, netPlayer);
				break;
		}
	}

	void UpdateNetObject(DataToUpdateNetObject dataToUpdate)
	{
		if (netObjects.ContainsKey(dataToUpdate.netID))
		{
			netObjects[dataToUpdate.netID].UpdateObjectData(dataToUpdate.objectData);
		}
	}

	// TO IMPLEMENT
	void DestroyNetObject(DataToDestroyNetObject dataToDestroy)
	{

	}

	ObjectStateSegment GetSegmentToRecreateNetObjectsDictionary(int netID)
	{
		ObjectStateSegment segment = new ObjectStateSegment(ObjectReplicationAction.RECREATE, netObjects[netID].GetDataToRecreate().Serialize());

		return segment;
	}

	public Packet GetNetObjectsPacket()
	{
		ObjectStatePacketBody body = new ObjectStatePacketBody();

		foreach (var key in netObjects.Keys)
		{
			body.AddSegment(GetSegmentToRecreateNetObjectsDictionary(key));
		}

		Packet packet = new Packet(PacketType.OBJECT_STATE, NetworkingEnd.instance.userID, body);

		return packet;
	}

	public void SearchNetObjectsInScene()
	{
		NetObject[] netObjectsInScene = (NetObject[])FindObjectsOfType(typeof(NetObject));

		foreach (var netObj in netObjectsInScene)
		{
			if (netObj.netID == -1)
			{
				netObj.netID = nextNetID;
				netObjects.Add(netObj.netID, netObj);
				Debug.Log("NetObject " + netObj.name + " stored with a netID of " + netObj.netID);
				nextNetID++;
			}
		}
	}

	//public void SendNetObjectsToAllUsers()
	//{
	//	NetworkingEnd.instance.SendPacketToAllUsers(PrepareLevelReplicationPacket());
	//}

	//Packet PrepareLevelReplicationPacket()
	//{
	//	string levelName = SceneManager.GetActiveScene().name;

	//	List<DataToRecreateNetObject> netObjetsToSend = new List<DataToRecreateNetObject>();

	//	foreach (var netObject in netObjects)
	//	{
	//		DataToRecreateNetObject dataToRecreate = netObject.Value.GetDataToRecreate();

	//		netObjetsToSend.Add(dataToRecreate);
	//	}

	//	LevelReplicationPacketBody body = new LevelReplicationPacketBody(levelName, netObjetsToSend);

	//	Packet packet = new Packet(PacketType.LEVEL_REPLICATION, NetworkingEnd.instance.userID, body);

	//	return packet;
	//}

	//public void ReplicateLevelObjects(List<DataToRecreateNetObject> objectsToRecreate)
	//{
	//	foreach (var objToRecreate in objectsToRecreate)
	//	{
	//		RecreateNetObject(objToRecreate.netID, objToRecreate.netClass, objToRecreate.data);
	//	}
	//}
}
