using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : NetworkingEnd
{
	public List<EndPoint> usersConnected = new List<EndPoint>();

	private void LateUpdate()
	{
		while (preparedPackets.Count > 0)
		{
			Packet packet = preparedPackets.Dequeue();

			foreach (EndPoint user in usersConnected)
			{
				Thread sendThread = new Thread(() => SendPacket(packet, user));
				sendThread.Start();
			}
		}
	}

	public override void StartLevel(Vector3 startPoint) 
	{
		NetObjectsManager.instance.CreateLocalPlayer();
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

	protected override void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress)
    {
		Debug.Log("Packet Received");

		Packet packet = new Packet(inputPacket);

		if (!usersConnected.Contains(fromAddress))
		{
			usersConnected.Add(fromAddress);
		}

		// host update stuff
		switch (packet.type)
		{
			case PacketType.HELLO:
				Debug.Log("Server Recieved HELLO");
				OnHelloPacketRecieved(packet, fromAddress);
				break;
			case PacketType.WELCOME:
				Debug.Log("Server Recieved WELCOME");
				OnWelcomePacketRecieved(packet, fromAddress);
				break;
			case PacketType.PING:
				Debug.Log("Server Recieved PING");
				OnPingPacketRecieved(packet, fromAddress);
				break;
			case PacketType.OBJECT_STATE:
				Debug.Log("Server Recieved OBJECT_STATE");
				OnObjectStatePacketRecieved(packet, fromAddress);
				break;
		}

		// TO-DO NOW DOESN'T WORK
		//// send the message to all users EXCEPT origin
		//foreach (EndPoint user in usersConnected)
		//{
		//	if (user == fromAddress) { continue; }
		//	Thread sendThread = new Thread(() => SendPacket(packet, user));
		//	sendThread.Start();
		//}
	}

	protected override void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress)
	{
		WelcomePacketBody body = new WelcomePacketBody(usersConnected.Count);
		Packet welcomePacket = new Packet(PacketType.WELCOME, userID, body);
		SendPacket(welcomePacket, fromAddress);

		ObjectStatePacketBody playerBodyPacket = new ObjectStatePacketBody();
		playerBodyPacket.AddSegment(ObjectReplicationAction.RECREATE, userID, ObjectReplicationClass.FOREIGN_PLAYER, new byte[1]);
		Packet playerPacket = new Packet(PacketType.OBJECT_STATE, userID, playerBodyPacket);
		SendPacket(playerPacket, fromAddress);
	}
	protected override void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress)
	{
		NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
	}

	public string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList) 
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new System.Exception("No network adapters with an IPv4 address in the system");
	}
}