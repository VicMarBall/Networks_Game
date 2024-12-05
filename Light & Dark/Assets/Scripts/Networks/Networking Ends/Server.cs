using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : NetworkingEnd
{
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

		SendPacket(NetObjectsManager.instance.GetNetObjectsPacket(), fromAddress);

		//ObjectStatePacketBody playerBodyPacket = new ObjectStatePacketBody();
		//playerBodyPacket.AddSegment(ObjectReplicationAction.RECREATE, userID, NetObjectClass.PLAYER, new byte[1]);
		//Packet playerPacket = new Packet(PacketType.OBJECT_STATE, userID, playerBodyPacket);
		//SendPacket(playerPacket, fromAddress);
	}
	protected override void OnWelcomePacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnPingPacketRecieved(Packet packet, EndPoint fromAddress) { }
	protected override void OnObjectStatePacketRecieved(Packet packet, EndPoint fromAddress)
	{
		NetObjectsManager.instance.ManageObjectStatePacket((ObjectStatePacketBody)packet.body);
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