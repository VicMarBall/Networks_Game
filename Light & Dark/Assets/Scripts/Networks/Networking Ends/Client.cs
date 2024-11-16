using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Client : NetworkingEnd
{
	IPEndPoint targetIPEP;

	private void LateUpdate()
	{
		while (preparedPackets.Count > 0)
		{
			Packet packet = preparedPackets.Dequeue();

			Thread sendThread = new Thread(() => SendPacket(packet, targetIPEP));
			sendThread.Start();
		}
	}


	public void StartClient(string ipTarget = "127.0.0.1")
	{
		// create socket and set the target IPEP
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		targetIPEP = new IPEndPoint(IPAddress.Parse(ipTarget), 9050);

		Debug.Log("Client Created");

		Thread mainThread = new Thread(ConnectToServer);
		mainThread.Start();
	}

	void ConnectToServer()
	{
		socket.Connect(targetIPEP);

		Debug.Log("Client Connected to Server");

		// send a first message to the server
		HelloPacketBody body = new HelloPacketBody();
		Packet packet = new Packet(PacketType.HELLO, userID, body);

		SendPacket(packet, targetIPEP);

		// start recieving messages
		Thread receiveThread = new Thread(ReceivePacket);
		receiveThread.Start();
	}

	protected override void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress)
	{
		Packet packet = new Packet(inputPacket);

		switch (packet.type)
		{
			case PacketType.HELLO:
				Debug.Log("Client Recieved HELLO");
				break;
			case PacketType.WELCOME:
				Debug.Log("Client Recieved WELCOME");
				WelcomePacketBody welcome = (WelcomePacketBody)packet.body;
				SetUserID(welcome.newPlayerID);
				break;
			case PacketType.PING:
				Debug.Log("Client Recieved PING");
				break;
			case PacketType.OBJECT_STATE:
				Debug.Log("Client Recieved OBJECT_STATE");
				NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
				break;
		}
	}

	public override void StartLevel(Vector3 startPoint)
	{
		ObjectStatePacketBody body = new ObjectStatePacketBody();
		body.AddSegment(ObjectReplicationAction.CREATE, userID, ObjectReplicationClass.FOREIGN_PLAYER, ObjectReplicationRegistry.SerializeVector3(startPoint));
		PreparePacket(new Packet(PacketType.OBJECT_STATE, userID, body));
	}
}
