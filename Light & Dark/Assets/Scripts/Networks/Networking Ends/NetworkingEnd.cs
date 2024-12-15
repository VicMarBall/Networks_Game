using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

abstract public class NetworkingEnd : MonoBehaviour
{
	#region SINGLETON
	public static NetworkingEnd instance { get; private set; }

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	#endregion
	public int userID { get; protected set; }

	protected Socket socket;

	// client: usersConnected = server / host
	// server / host: usersConnected = all clients
	protected List<EndPoint> usersConnected = new List<EndPoint>();

	protected Queue<Packet> preparedPackets = new Queue<Packet>();

	public abstract bool IsServer();

	protected void ReceivePacket()
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

	protected void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress)
	{
		Debug.Log("Packet Received");

		Packet packet = new Packet(inputPacket);

		// host update stuff
		switch (packet.type)
		{
			case PacketType.HELLO:
				Debug.Log("Recieved HELLO");
				OnHelloPacketRecieved(packet, fromAddress);
				break;
			case PacketType.WELCOME:
				Debug.Log("Recieved WELCOME");
				OnWelcomePacketRecieved(packet, fromAddress);
				break;
			case PacketType.PING:
				Debug.Log("Recieved PING");
				OnPingPacketRecieved(packet, fromAddress);
				break;
			case PacketType.OBJECT_STATE:
				Debug.Log("Recieved OBJECT_STATE");
				OnObjectStatePacketRecieved(packet, fromAddress);
				break;
			case PacketType.REQUEST:
				Debug.Log("Recieved REQUEST");
				OnRequestPacketRecieved(packet, fromAddress);
				break;
		}

		// send the message to all users EXCEPT origin
		foreach (EndPoint user in usersConnected)
		{
			if (user.Equals(fromAddress)) { continue; }
			Thread sendThread = new Thread(() => SendPacket(packet, user));
			sendThread.Start();
		}
	}

	virtual protected void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress) { }
	virtual protected void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) { }
	virtual protected void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) { }
	virtual protected void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress) { }
	virtual protected void OnRequestPacketRecieved(Packet packet, EndPoint fromAddress) { }

	protected void SendPacket(Packet packet, EndPoint target)
	{
		Debug.Log(packet.type + " Packet Sent");
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

	public void SendPacketToAllUsers(Packet packet) 
	{
		foreach (EndPoint user in usersConnected)
		{
			Thread sendThread = new Thread(() => SendPacket(packet, user));
			sendThread.Start();
		}
	}

	virtual public void StartLevel(Vector3 startPoint) { }
}
