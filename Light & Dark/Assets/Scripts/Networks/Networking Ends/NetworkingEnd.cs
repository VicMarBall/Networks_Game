using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

abstract public class NetworkingEnd : MonoBehaviour
{
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

	abstract protected void OnPacketRecieved(byte[] inputPacket, EndPoint fromAddress);

	protected void SendPacket(Packet packet, EndPoint target)
	{
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

}
