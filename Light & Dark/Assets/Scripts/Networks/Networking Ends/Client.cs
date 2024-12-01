using System.Collections;
using System.Collections.Generic;
using System.IO;
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
				OnHelloPacketRecieved(packet, fromAddress);
				break;
			case PacketType.WELCOME:
				Debug.Log("Client Recieved WELCOME");
				OnWelcomePacketRecieved(packet, fromAddress);
				break;
			case PacketType.PING:
				Debug.Log("Client Recieved PING");
				OnPingPacketRecieved(packet, fromAddress);
				break;
			case PacketType.OBJECT_STATE:
				Debug.Log("Client Recieved OBJECT_STATE");
				OnObjectStatePacketRecieved(packet, fromAddress);
				break;
		}
	}

	protected override void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		WelcomePacketBody welcome = (WelcomePacketBody)packet.body;
		SetUserID(welcome.newPlayerID);
	}
	protected override void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
	}


	public override void StartLevel(Vector3 startPoint)
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(userID);

		writer.Write(startPoint.x);
		writer.Write(startPoint.y);
		writer.Write(startPoint.z);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();


		NetObjectsManager.instance.PrepareNetObjectCreate(NetObjectClass.PLAYER, objectAsBytes);
	}
}
