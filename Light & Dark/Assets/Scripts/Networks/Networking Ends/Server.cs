using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : NetworkingEnd
{
	struct RequestReceived
	{
		public RequestPacketBody request;
		public EndPoint endPoint;
	}

	Queue<RequestReceived> requestsRecieved = new Queue<RequestReceived>();

	private void Update()
	{
		while (requestsRecieved.Count != 0)
		{
			RequestReceived request = requestsRecieved.Dequeue();

			switch (request.request.requestType)
			{
				case RequestType.LEVEL_REPLICATION:
					SendPacket(NetObjectsManager.instance.GetNetObjectsPacket(), request.endPoint);
					RequestPacketBody createPlayerRequest = new RequestPacketBody(RequestType.CREATE_PLAYER);
					Packet createPlayerPacket = new Packet(PacketType.REQUEST, userID, createPlayerRequest);
					SendPacket(createPlayerPacket, request.endPoint);
					break;
			}
		}
	}

	public void StartServer()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint IPEP = new IPEndPoint(IPAddress.Any, 9050);
		socket.Bind(IPEP);

		Debug.Log("Server Started");

		// TO-MOVE
		GameManager.instance.SetEnvironmentView(GameplayEnvironmentMode.DARK);

		Thread newConnection = new Thread(ReceivePacket);
		newConnection.Start();
	}

	protected override void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress)
	{
		if (!usersConnected.Contains(fromAddress))
		{
			usersConnected.Add(fromAddress);
		}

		WelcomePacketBody body = new WelcomePacketBody(usersConnected.Count);
		Packet welcomePacket = new Packet(PacketType.WELCOME, userID, body);
		SendPacket(welcomePacket, fromAddress);
	}
	protected override void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress)
	{
		NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
	}
	protected override void OnRequestPacketRecieved(Packet packet, EndPoint fromAddress)
	{
		RequestType requestType = ((RequestPacketBody)packet.body).requestType;

		switch (requestType)
		{
			case RequestType.LEVEL_REPLICATION:
				RequestReceived requestReceived = new RequestReceived();
				requestReceived.request = (RequestPacketBody)packet.body;
				requestReceived.endPoint = fromAddress;
				requestsRecieved.Enqueue(requestReceived);
				break;
		}
	}

	public override void StartLevel(Vector3 startPoint)
	{
		PlayerData playerData = new PlayerData();
		playerData.position = startPoint;
		playerData.rotation = Vector3.zero;
		playerData.scale = Vector3.one;

		DataToCreateNetObject dataToCreate = new DataToCreateNetObject();
		dataToCreate.ownerID = userID;
		dataToCreate.netClass = NetObjectClass.PLAYER;
		dataToCreate.objectData = playerData.Serialize();

		NetObjectsManager.instance.CreateNetObjectFromLocal(dataToCreate);
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

	public override bool IsServer()
	{
		return true;
	}
}