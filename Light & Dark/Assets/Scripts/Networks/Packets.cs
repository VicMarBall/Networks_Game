using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketBodyType
{
	HELLO, // client requests to server entrance
	PING, // 
	OBJECT_STATE
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

	byte[] Serialize() 
	{
		// TO IMPLEMENT


		return null;
	}

	void Deserialize(byte[] data) 
	{
		// deserialize first value // TO IMPLEMENT

		// deserialize second value // TO IMPLEMENT

		switch (type)
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
			default:
				break;
		}
	}
}

public abstract class PacketBody
{
	public abstract void Deserialize(byte[] data);
}


public class HelloPacketBody : PacketBody
{
	public override void Deserialize(byte[] data) { }
}

public class PingPacketBody : PacketBody
{
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
	public override void Deserialize(byte[] data) 
	{ 
		// TO IMPLEMENT
	}
}