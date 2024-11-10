using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectStatePacketBody;

public class NetObjectsManager : MonoBehaviour
{
	// singleton
	public static NetObjectsManager instance { get; private set; }

	[SerializeField]
	GameObject player2Prefab;

	Dictionary<int, GameObject> networkObjects;

	List<ObjectStatePacketBodySegment> preparedPacketBodies;

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

	public void ReceivePlayerPacket(int netID, PlayerPacketBody packet)
	{
		switch (packet.action)
		{
			case ObjectReplicationAction.CREATE:
				CreatePlayer(netID, packet);
				break;
			case ObjectReplicationAction.UPDATE:
				UpdatePlayer(netID, packet);
				break;
			case ObjectReplicationAction.DESTROY:
				Debug.Log("Destroy Player: " + netID);
				break;
			default:
				Debug.Log("Action not found");
				break;
		}
	}

	void CreatePlayer(int netID, PlayerPacketBody packet)
	{
		GameObject player2 = Instantiate(player2Prefab);
		networkObjects.Add(netID, player2);

		networkObjects[netID].transform.position = packet.transform.position;
		networkObjects[netID].transform.rotation = packet.transform.rotation;
		networkObjects[netID].transform.localScale = packet.transform.localScale;
	}

	void UpdatePlayer(int netID, PlayerPacketBody packet)
	{
		networkObjects[netID].transform.position = packet.transform.position;
		networkObjects[netID].transform.rotation = packet.transform.rotation;
		networkObjects[netID].transform.localScale = packet.transform.localScale;
	}

	public void UpdateNetworkObjects(ObjectStatePacketBody packetBody)
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
