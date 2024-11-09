using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
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

	public PacketBodyType GetPacketType() {  return type; }
	public PacketBody GetBody() { return body; }

	public byte[] Serialize() 
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((int)type);
		writer.Write(playerID);
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

		type = (PacketBodyType)reader.ReadInt32();

		playerID = reader.ReadInt32();

		int bodyLength = (int)(stream.Length - stream.Position);

		switch (type)
		{
			case PacketBodyType.HELLO:
				body = new HelloPacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketBodyType.PING:
				body = new PingPacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketBodyType.OBJECT_STATE:
				body = new ObjectStatePacketBody(reader.ReadBytes(bodyLength));
				break;
			case PacketBodyType.TESTING:
				body = new ObjectStatePacketBody(reader.ReadBytes(bodyLength));
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
	// send
	public HelloPacketBody() { }

	// recieve
	public HelloPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public override byte[] Serialize() { return null; }
	public override void Deserialize(byte[] data) { }
}

public class PingPacketBody : PacketBody
{
	// send
	public PingPacketBody() { }

	// recieve
	public PingPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public override byte[] Serialize() { return null; }
	public override void Deserialize(byte[] data) { }
}

public enum ObjectReplicationAction
{
	CREATE,
	UPDATE,
	DESTROY
}


// NOT USE YET
public struct ObjectStatePacketBodySegment
{
	public ObjectStatePacketBodySegment(ObjectReplicationAction action, int netID, ObjectReplicationClass objectClass, byte[] data)
	{
		this.action = action;
		this.netID = netID;
		this.objectClass = objectClass;
		this.data = data;
	}

	public ObjectReplicationAction action;
	public int netID;
	public ObjectReplicationClass objectClass;
	public byte[] data; // this data is serialized with functions in ObjectReplicationRegistry.cs

	public byte[] Serialize() 
	{
		// TO IMPLEMENT

		return null;
	}
	public void Deserialize(byte[] data) 
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		action = (ObjectReplicationAction)reader.ReadInt32();
		netID = reader.ReadInt32();
		objectClass = (ObjectReplicationClass)reader.ReadInt32();

		//
	}
}

// NO USE YET
public class ObjectStatePacketBody : PacketBody
{
	public List<ObjectStatePacketBodySegment> segments;

	// send
	public ObjectStatePacketBody() { }

	// recieve
	public ObjectStatePacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public void AddSegment(ObjectReplicationAction action, int netID, ObjectReplicationClass objectClass, byte[] data)
	{
		segments.Add(new ObjectStatePacketBodySegment(action, netID, objectClass, data));
	}
	public void AddSegment(ObjectStatePacketBodySegment segment)
	{
		segments.Add(segment);
	}

	public override byte[] Serialize() 
	{
		// TO IMPLEMENT
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(segments.Count);

		foreach (var segment in segments)
		{
			writer.Write(segment.Serialize());
		}

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	public override void Deserialize(byte[] data) 
	{
		// TO IMPLEMENT
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		int nSegments = reader.ReadInt32();

		for (int i = 0; i < nSegments; ++i)
		{

		}
	}
}

public class TestingPacketBody : PacketBody
{
	string testString = "Test Packet";
	public override byte[] Serialize() 
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(testString);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes; 
	}
	public override void Deserialize(byte[] data) 
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);
		
		testString = reader.ReadString();
	}
}