using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Client : MonoBehaviour
{
	int playerID = 0;

	Socket socket;
	IPEndPoint targetIPEP;

	Thread mainThread;
	Thread receiveThread;

	Dictionary<int, GameObject> networkObjects;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
		string ipTarget = "127.0.0.0";

		// create socket and set the target IPEP
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		targetIPEP = new IPEndPoint(IPAddress.Parse(ipTarget), 9050);
		socket.Connect(targetIPEP);

		// send a first message to the server
		TestingPacketBody body = new TestingPacketBody();
		Packet packet = new Packet(PacketBodyType.TESTING, playerID, body);

		SendPacket(packet, targetIPEP);

		// start recieving messages
		receiveThread = new Thread(ReceiveMessages);
		receiveThread.Start();
	}

	void ReceiveMessages()
	{
		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint remote = (EndPoint)(sender);

		byte[] data = new byte[1024];

		while (true)
		{
			int recv = socket.ReceiveFrom(data, ref remote);
			OnPacketRecieved(data, recv);
		}
	}

	void SendPacket(Packet packet, IPEndPoint target)
	{
		byte[] data = packet.Serialize();
		socket.SendTo(data, target);
	}

	void OnPacketRecieved(byte[] inputPacket, int packetLength)
	{
		Packet packet = new Packet(inputPacket);
	}


	void DeserializeWelcomePacket(byte[] welcomePacket)
    {

    }

    // TO IMPLEMENT
    void UpdateNetworkObject(int netID, byte[] data)
    {

    }
}
