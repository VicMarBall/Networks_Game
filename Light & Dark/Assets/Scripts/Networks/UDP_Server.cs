using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using System.Numerics;
using System.IO;

public class UDP_Server : MonoBehaviour
{
	Socket socket;

	string serverName = "Server";
	
	public List<EndPoint> usersConnected = new List<EndPoint>();

	void Update()
	{

	}

	public void StartServer()
	{
		serverText = "Starting UDP Server...\n";

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

		serverText += "Waiting for new Client...\n";

		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint remote = (EndPoint)(sender);

		while (true)
		{
			recv = socket.ReceiveFrom(data, ref remote);

			if (!usersConnected.Contains(remote))
			{
				usersConnected.Add(remote);
				Thread sendThread = new Thread(() => SendStringToEP(remote, "Thanks for joining " + serverName + " server!\n"));
				sendThread.Start();
			}

			ChatMessage message = new ChatMessage();
			message.Deserialize(data);
			serverText += message.ToString();

			// send the message to all users
			foreach (EndPoint user in usersConnected)
			{
				Thread sendThread = new Thread(() => SendStringToEP(user, message.ToString()));
				sendThread.Start();
			}
		}

	}

	void SendStringToEP(EndPoint remote, string lastMessage)
	{
		try
		{
			byte[] data = Encoding.ASCII.GetBytes(lastMessage);
			socket.SendTo(data, remote);
		}
		catch (System.Exception e)
		{
			Debug.Log(e.ToString());
			throw;
		}
	}
}
