using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Host : NetworkingEnd
{
	int playerID = 0;

	public List<EndPoint> usersConnected = new List<EndPoint>();

	public void StartHost()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint IPEP = new IPEndPoint(IPAddress.Any, 9050);
		socket.Bind(IPEP);

		Debug.Log("Host Started");

		Thread newConnection = new Thread(ReceivePacket);
		newConnection.Start();
	}

	protected override void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress)
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
				NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.GetBody());
				break;
			case PacketBodyType.TESTING:
				NetObjectsManager.instance.TestManager();
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
}
