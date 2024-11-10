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
				NetObjectsManager.instance.UpdateNetworkObjects((ObjectStatePacketBody)packet.GetBody());
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