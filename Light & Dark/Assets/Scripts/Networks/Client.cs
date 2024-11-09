using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Client : MonoBehaviour
{
	int playerID = 0;

	Socket socket;
	IPEndPoint targetIPEP;

	Thread mainThread;
	Thread receiveThread;

	public void StartClient()
	{
		string ipTarget = "127.0.0.1";

		// create socket and set the target IPEP
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		targetIPEP = new IPEndPoint(IPAddress.Parse(ipTarget), 9050);

		Debug.Log("Client Created");

		mainThread = new Thread(ConnectToServer);
		mainThread.Start();
	}

	void ConnectToServer()
	{
		socket.Connect(targetIPEP);

		Debug.Log("Client Connected to Server");

		// send a first message to the server
		// TO DO change it to a hello packet
		TestingPacketBody body = new TestingPacketBody();
		Packet packet = new Packet(PacketBodyType.TESTING, playerID, body);

		SendPacket(packet, targetIPEP);

		// start recieving messages
		receiveThread = new Thread(ReceiveMessages);
		receiveThread.Start();
	}

	void ReceiveMessages()
	{
		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint remote = (EndPoint)(sender);

		byte[] data = new byte[1024];

		while (true)
		{
			int recv = socket.ReceiveFrom(data, ref remote);
			OnPacketRecieved(data, recv);
		}
	}

	void SendPacket(Packet packet, IPEndPoint target)
	{
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

	void OnPacketRecieved(byte[] inputPacket, int packetLength)
	{
		Packet packet = new Packet(inputPacket);

		switch (packet.GetPacketType())
		{
			case PacketBodyType.HELLO:
				break;
			case PacketBodyType.PING:
				break;
			case PacketBodyType.OBJECT_STATE:
				NetObjectsManager.instance.UpdateNetworkObjects((ObjectStatePacketBody)packet.GetBody());
				break;
			case PacketBodyType.TESTING:
				NetObjectsManager.instance.TestManager();
				break;
		}
	}
}
