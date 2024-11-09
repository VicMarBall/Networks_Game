using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using static ObjectStatePacketBody;

public class Server : MonoBehaviour
{
	Socket socket;

	public List<EndPoint> usersConnected = new List<EndPoint>();

	Dictionary<int, GameObject> networkObjects;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void StartServer()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint IPEP = new IPEndPoint(IPAddress.Any, 9050);
		socket.Bind(IPEP);

		Debug.Log("Server Started");

		Thread newConnection = new Thread(ReceivePacket);
		newConnection.Start();
	}

	void ReceivePacket()
	{
		int recv;
		byte[] data = new byte[1024];

		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint remote = (EndPoint)(sender);

		while (true)
		{
			Debug.Log("Waiting Packet");

			recv = socket.ReceiveFrom(data, ref remote);

			OnPacketRecieved(data, remote);
		}

	}

	void SendPacket(Packet packet, EndPoint target)
	{
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

	void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress)
    {
		Debug.Log("Packet Received");

		Packet packet = new Packet(inputPacket);

		if (packet.GetPacketType() == PacketBodyType.TESTING)
		{
			Debug.Log("Testing Packet Received");
		}

		// host update stuff
		switch (packet.GetPacketType())
		{
			case PacketBodyType.HELLO:
				break;
			case PacketBodyType.PING:
				break;
			case PacketBodyType.OBJECT_STATE:
				UpdateNetworkObjects((ObjectStatePacketBody)packet.GetBody());
				break;
			case PacketBodyType.TESTING:
				break;
		}

		// send the message to all users EXCEPT origin
		foreach (EndPoint user in usersConnected)
		{
			if (user == fromAddress) { continue; }
			Thread sendThread = new Thread(() => SendPacket(packet, user));
			sendThread.Start();
		}
	}


	void UpdateNetworkObjects(ObjectStatePacketBody packetBody)
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
