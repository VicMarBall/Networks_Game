using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : NetworkingEnd
{
	IPEndPoint targetIPEP;

	struct RequestReceived
	{
		public RequestPacketBody request;
		public EndPoint endPoint;
	}

	Queue<RequestReceived> requestsRecieved = new Queue<RequestReceived>();

	public float latency { get; protected set; }
	public float jitter { get; protected set; } = 0;
	float lastPingReceived = 0;

	private void Update()
	{
		lastPingReceived += Time.deltaTime;

		while (requestsRecieved.Count != 0)
		{
			RequestReceived request = requestsRecieved.Dequeue();

			switch (request.request.requestType)
			{
				case RequestType.CREATE_PLAYER:
					PlayerData playerData = new PlayerData();
					playerData.position = new Vector3(0, 0, 0);
					playerData.rotation = Vector3.zero;
					playerData.scale = Vector3.one;

					DataToCreateNetObject dataToCreate = new DataToCreateNetObject();
					dataToCreate.ownerID = userID;
					dataToCreate.netClass = NetObjectClass.PLAYER;
					dataToCreate.objectData = playerData.Serialize();

					NetObjectsManager.instance.ReceiveObjectStateToSend(-1, new ObjectStateSegment(ObjectReplicationAction.CREATE, dataToCreate.Serialize()));

					break;
			}
		}
	}

	public void StartClient(string ipTarget = "127.0.0.1")
	{
		// create socket and set the target IPEP
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		targetIPEP = new IPEndPoint(IPAddress.Parse(ipTarget), 9050);

		Debug.Log("Client Created");

		// TO-MOVE
		GameManager.instance.SetEnvironmentView(GameplayEnvironmentMode.LIGHT);

		Thread mainThread = new Thread(ConnectToServer);
		mainThread.Start();
	}

	void ConnectToServer()
	{
		socket.Connect(targetIPEP);

		Debug.Log("Client Connected to Server");

		usersConnected.Add(targetIPEP);

		// send a first message to the server
		HelloPacketBody body = new HelloPacketBody();
		Packet packet = new Packet(PacketType.HELLO, userID, body);

		SendPacket(packet, targetIPEP);

		// start recieving messages
		Thread receiveThread = new Thread(ReceivePacket);
		receiveThread.Start();
	}

	protected override void OnHelloPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		WelcomePacketBody welcome = (WelcomePacketBody)packet.body;
		userID = welcome.newPlayerID;
	}
	protected override void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		PingPacketBody ping = (PingPacketBody)packet.body;

		jitter += (Mathf.Abs(lastPingReceived - pingIntervalTime)) * 0.5f;
		latency = ping.latency;
		Debug.Log("Latency " + (latency * 1000) + "ms");
		lastPingReceived = 0;

		PongPacketBody pong = new PongPacketBody();
		Packet pongPacket = new Packet(PacketType.PONG, userID, pong);
		SendPacket(pongPacket, fromAddress);
	}
	protected override void OnPongPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
	}

	protected override void OnRequestPacketRecieved(Packet packet, EndPoint fromAddress)
	{
		RequestType requestType = ((RequestPacketBody)packet.body).requestType;

		switch (requestType)
		{
			case RequestType.CREATE_PLAYER:
				RequestReceived requestReceived = new RequestReceived();
				requestReceived.request = (RequestPacketBody)packet.body;
				requestReceived.endPoint = fromAddress;
				requestsRecieved.Enqueue(requestReceived);
				break;
		}
	}
	protected override void OnByePacketRecieved(Packet packet, EndPoint fromAddress) 
	{
		ByePacketBody bye = (ByePacketBody)packet.body;

		if (bye.fromServer)
		{
			OnServerDisconnects(packet.originPlayerID);
		}
		else
		{
			OnOtherClientDisconnects(packet.originPlayerID);
		}
	}


	public override void StartLevel(Vector3 startPoint)
	{
		RequestPacketBody body = new RequestPacketBody(RequestType.LEVEL_REPLICATION);
		Packet requestLevelReplicationPacket = new Packet(PacketType.REQUEST, userID, body);
		SendPacket(requestLevelReplicationPacket, targetIPEP);
	}

	public override bool IsServer()
	{
		return false;
	}

	void DisconnectFromServer()
	{
		ByePacketBody bye = new ByePacketBody(IsServer());
		Packet packet = new Packet(PacketType.BYE, userID, bye);
		SendPacketToAllUsers(packet);

		try
		{
			socket.Shutdown(SocketShutdown.Both);
		}
		finally
		{
			socket.Close();
		}
	}

	void OnServerDisconnects(int serverID)
	{
		NetObjectsManager.instance.DestroyPlayer(serverID);

		usersConnected.Clear();
	}

	void OnOtherClientDisconnects(int userID)
	{
		NetObjectsManager.instance.DestroyPlayer(userID);
	}

	private void OnDestroy()
	{
		DisconnectFromServer();
	}
}
