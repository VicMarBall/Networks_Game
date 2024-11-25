using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

abstract public class NetworkingEnd : MonoBehaviour
{
	public int userID {  get; private set; }
	// singleton
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

	protected Socket socket;

	protected Queue<Packet> preparedPackets = new Queue<Packet>();

	protected void SetUserID(int userID)
	{
		this.userID = userID;
	}

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

	public void PreparePacket(Packet packet) 
	{
		preparedPackets.Enqueue(packet);
	}

	abstract protected void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress);

	virtual public void StartLevel(Vector3 startPoint) { }

	protected void SendPacket(Packet packet, EndPoint target)
	{
		Debug.Log(packet.type + " Packet Sent");
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

	virtual protected void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress) { }
	virtual protected void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress)	{ }
	virtual protected void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) { }
	virtual protected void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress) { }
}
