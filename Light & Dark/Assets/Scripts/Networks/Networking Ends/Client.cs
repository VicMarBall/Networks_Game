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
	int playerID = 0;

	IPEndPoint targetIPEP;

	public void StartClient()
	{
		string ipTarget = "127.0.0.1";

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
		Packet packet = new Packet(PacketBodyType.HELLO, playerID, body);

		SendPacket(packet, targetIPEP);

		// start recieving messages
		Thread receiveThread = new Thread(ReceivePacket);
		receiveThread.Start();
	}

	protected override void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress)
	{
		Packet packet = new Packet(inputPacket);

		switch (packet.GetPacketType())
		{
			case PacketBodyType.HELLO:
				break;
			case PacketBodyType.WELCOME:
				WelcomePacketBody welcome = (WelcomePacketBody)packet.GetBody();
				GameManager.instance.SetPlayerID(welcome.newPlayerID);
				break;
			case PacketBodyType.PING:
				break;
			case PacketBodyType.OBJECT_STATE:
				NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.GetBody());
				break;
			case PacketBodyType.TESTING:
				NetObjectsManager.instance.TestManager();
				break;
		}
	}
}
