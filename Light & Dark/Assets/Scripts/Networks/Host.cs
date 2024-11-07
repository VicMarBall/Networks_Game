using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.tvOS;

public class Host : MonoBehaviour
{
	Socket socket;


	public List<EndPoint> usersConnected = new List<EndPoint>();

	Dictionary<int, GameObject> networkObjects;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void StartServer()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint IPEP = new IPEndPoint(IPAddress.Any, 9050);
		socket.Bind(IPEP);

		Thread newConnection = new Thread(ReceiveChatMessage);
		newConnection.Start();
	}

	void ReceiveChatMessage()
	{
		int recv;
		byte[] data = new byte[1024];

		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint remote = (EndPoint)(sender);

		while (true)
		{
			recv = socket.ReceiveFrom(data, ref remote);

			Packet packet = new Packet(data);

			if (!usersConnected.Contains(remote))
			{
				usersConnected.Add(remote);
				//Thread sendThread = new Thread(() => SendPacket(remote, "Thanks for joining " + serverName + " server!\n"));
				//sendThread.Start();
			}

		}

	}

	void SendPacket(Packet packet, EndPoint target)
	{
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

	void OnPacketRecieved(byte[] inputPacket, Socket fromAddress)
    {
        Packet packet = new Packet(inputPacket);

		// host update stuff

		// send the message to all users EXCEPT origin
		foreach (EndPoint user in usersConnected)
		{
			if (user == fromAddress.LocalEndPoint) { continue; }
			Thread sendThread = new Thread(() => SendPacket(packet, user));
			sendThread.Start();
		}

	}
}
