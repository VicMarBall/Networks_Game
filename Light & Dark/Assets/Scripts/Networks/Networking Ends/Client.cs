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

		usersConnected.Add(targetIPEP);

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

	protected override void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		WelcomePacketBody welcome = (WelcomePacketBody)packet.body;
		userID = welcome.newPlayerID;
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
