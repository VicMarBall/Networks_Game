using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class UDP_Client : MonoBehaviour
{
	Socket socket;
	IPEndPoint targetIPEP;

	string clientName = "noname";

	string ipTarget = "127.0.0.1";

	Thread mainThread;
	Thread receiveThread;

	void Update()
	{

    }

	public void StartClient()
	{
		mainThread = new Thread(ConnectToServer);
		mainThread.Start();
	}

	void ConnectToServer()
	{
		try
		{
			// create socket and set the target IPEP
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			targetIPEP = new IPEndPoint(IPAddress.Parse(ipTarget), 9050);
			socket.Connect(targetIPEP);

			// send a first message to the server
			// send a hello packet
			//ChatMessage pongMessage = new ChatMessage(clientName + " hopped into the server!\n", "");
			//byte[] data = pongMessage.Serialize();
			//socket.SendTo(data, targetIPEP);

			// start recieving messages
			receiveThread = new Thread(ReceiveMessages);
			receiveThread.Start();
		}
		catch (System.Exception e)
		{
			Debug.Log(e.ToString());
			throw;
		}
	}

	public void StartSendChatMessage()
	{
		string message = chatInputField.text;
		chatInputField.text = string.Empty;

		mainThread = new Thread(() => SendChatMessage(message));
		mainThread.Start();
	}

	void SendChatMessage(string message)
	{
		ChatMessage chatMessage = new ChatMessage(message, clientName + " (" + targetIPEP.Address.ToString() + ")");
		byte[] data = chatMessage.Serialize();
		socket.SendTo(data, targetIPEP);
	}

	void ReceiveMessages()
	{
		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint remote = (EndPoint)(sender);

		byte[] data = new byte[1024];

		while (true)
		{
			int recv = socket.ReceiveFrom(data, ref remote);
			clientText += Encoding.ASCII.GetString(data, 0, recv);
		}
	}
}
