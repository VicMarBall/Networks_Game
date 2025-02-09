using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public enum PacketType
{
	HELLO,				// client enters the server
	WELCOME,			// server sends start information to client
	PING,				// constant message to make sure the connection is not lost
	PONG,				// response to PING
	OBJECT_STATE,		// sends what to do with an object
	REQUEST,			// request for other packets (mainly the world replication packet)
	LOBBY_ACTION,		// for actions inside the lobby
	BYE					// client = disconnects, server = shuts down
}

public class Packet
{
	// constructor to send
	// constructor to recieve
	public Packet(PacketType type, int originPlayerID, PacketBody body)
	{
		this.type = type;
		this.originPlayerID = originPlayerID;
		this.body = body;
	}
	public Packet(byte[] data)
	{
		Deserialize(data);
	}

	public PacketType type { get; private set; }
	public int originPlayerID { get; private set; }
	public PacketBody body { get; private set; }

	public byte[] Serialize() 
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((int)type);
		writer.Write(originPlayerID);
		writer.Write(body.Serialize());

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	void Deserialize(byte[] data) 
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		type = (PacketType)reader.ReadInt32();
		originPlayerID = reader.ReadInt32();

		int bodyLength = (int)(stream.Length - stream.Position);

		switch (type)
		{
			case PacketType.HELLO:
				body = new HelloPacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketType.WELCOME:
				body = new WelcomePacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketType.PING:
				body = new PingPacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketType.PONG:
				body = new PongPacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketType.OBJECT_STATE:
				body = new ObjectStatePacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketType.REQUEST:
				body = new RequestPacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketType.BYE:
				body = new ByePacketBody(reader.ReadBytes(bodyLength));
				break;
			default:
				break;
		}

		stream.Close();
	}
}