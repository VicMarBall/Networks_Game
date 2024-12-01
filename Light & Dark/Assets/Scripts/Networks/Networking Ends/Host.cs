using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

// NOT TO USE
public class Host : NetworkingEnd
{
	public void StartHost()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint IPEP = new IPEndPoint(IPAddress.Any, 9050);
		socket.Bind(IPEP);

		Debug.Log("Host Started");

		Thread newConnection = new Thread(ReceivePacket);
		newConnection.Start();
	}
}
