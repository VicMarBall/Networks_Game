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

	private void Update()
	{
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
			case RequestType.CREATE_PLAYER:
				RequestReceived requestReceived = new RequestReceived();
				requestReceived.request = (RequestPacketBody)packet.body;
				requestReceived.endPoint = fromAddress;
				requestsRecieved.Enqueue(requestReceived);
				break;
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
}
