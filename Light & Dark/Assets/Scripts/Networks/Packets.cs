using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

public enum PacketBodyType
{
	HELLO, // client requests to server entrance
	PING, // 
	OBJECT_STATE,
	TESTING
}

public class Packet
{
	// send
	public Packet(PacketBodyType packetType, int originPlayerID, PacketBody packetBody)
	{
		type = packetType;
		playerID = originPlayerID;
		body = packetBody;
	}

	// recieve
	public Packet(byte[] data)
	{
		Deserialize(data);
	}

	PacketBodyType type;
	int playerID;
	PacketBody body;

	public byte[] Serialize() 
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((byte)type);
		writer.Write(playerID);
		writer.Write(body.Serialize());

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	void Deserialize(byte[] data) 
	{
		// deserialize first value // TO IMPLEMENT

		// deserialize second value // TO IMPLEMENT

		PacketBodyType packetType = PacketBodyType.OBJECT_STATE;

		switch (packetType)
		{
			case PacketBodyType.HELLO:
				body = new HelloPacketBody();
				break;
			case PacketBodyType.PING:
				body = new PingPacketBody();
				break;
			case PacketBodyType.OBJECT_STATE:
				body = new ObjectStatePacketBody();
				break;
			case PacketBodyType.TESTING:
				body = new ObjectStatePacketBody();
				break;
			default:
				break;
		}
	}
}

public abstract class PacketBody
{
	public abstract byte[] Serialize();
	public abstract void Deserialize(byte[] data);
}


public class HelloPacketBody : PacketBody
{
	public override byte[] Serialize() { return null; }
	public override void Deserialize(byte[] data) { }
}

public class PingPacketBody : PacketBody
{
	public override byte[] Serialize() { return null; }
	public override void Deserialize(byte[] data) { }
}

public enum ObjectReplicationAction
{
	CREATE,
	UPDATE,
	DESTROY
}

public class ObjectStatePacketBody : PacketBody
{
	List<ObjectStatePacketBodySegment> segments;

	struct ObjectStatePacketBodySegment {
		ObjectReplicationAction action;
		int networkObjectID;
		ObjectReplicationClass objectClass;
		byte[] data;
	}
	public override byte[] Serialize() { return null; }

	public override void Deserialize(byte[] data) 
	{ 
		// TO IMPLEMENT
	}
}

public class TestingPacketBody : PacketBody
{
	string testString = "Test Packet";
	public override byte[] Serialize() { return null; }
	public override void Deserialize(byte[] data) { }
}