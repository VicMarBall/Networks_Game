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
		Packet packet = new Packet(PacketType.HELLO, playerID, body);

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
				break;
			case PacketType.WELCOME:
				WelcomePacketBody welcome = (WelcomePacketBody)packet.body;
				GameManager.instance.SetPlayerID(welcome.newPlayerID);
				break;
			case PacketType.PING:
				break;
			case PacketType.OBJECT_STATE:
				NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
				break;
		}
	}
}
