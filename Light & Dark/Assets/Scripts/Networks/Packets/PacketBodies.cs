using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class PacketBody
{
	public abstract byte[] Serialize();
	public abstract void Deserialize(byte[] data);
}

// ---------------------------------------------------------------------------
public class HelloPacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public HelloPacketBody()
	{
		
	}
	public HelloPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public override byte[] Serialize() 
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		// 

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public override void Deserialize(byte[] data) 
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		// 

		stream.Close();
	}
}

// ---------------------------------------------------------------------------
public class WelcomePacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public WelcomePacketBody(int newPlayerID)
	{
		this.newPlayerID = newPlayerID;
	}
	public WelcomePacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public int newPlayerID { get; private set; }

	public override byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(newPlayerID);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public override void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		newPlayerID = reader.ReadInt32();

		reader.Close();
	}
}

// ---------------------------------------------------------------------------
// NO FUNCTIONALITY
public class PingPacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public PingPacketBody()
	{

	}
	public PingPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public override byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		// 

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public override void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		// 

		stream.Close();
	}
}

// ---------------------------------------------------------------------------
public enum ObjectReplicationAction
{
	CREATE,
	RECREATE,
	UPDATE,
	DESTROY
}

public struct ObjectStatePacketBodySegment
{
	public ObjectStatePacketBodySegment(ObjectReplicationAction action, int netID, ObjectReplicationClass objectClass, byte[] objectData)
	{
		this.action = action;
		this.netID = netID;
		this.objectClass = objectClass;
		this.objectData = objectData;
	}

	public ObjectReplicationAction action { get; private set; }
	public int netID { get; private set; }
	public ObjectReplicationClass objectClass { get; private set; }
	public byte[] objectData { get; private set; }

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((int)action);
		writer.Write(netID);
		writer.Write((int)objectClass);
		writer.Write(objectData);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		action = (ObjectReplicationAction)reader.ReadInt32();
		netID = reader.ReadInt32();
		objectClass = (ObjectReplicationClass)reader.ReadInt32();

		int objectDataLength = (int)(stream.Length - stream.Position);
		objectData = reader.ReadBytes(objectDataLength);

		stream.Close();
	}
}

public class ObjectStatePacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public ObjectStatePacketBody() 
	{
		
	}
	public ObjectStatePacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public List<ObjectStatePacketBodySegment> segments = new List<ObjectStatePacketBodySegment>();

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
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(segments.Count);

		foreach (var segment in segments)
		{
			byte[] segmentData = segment.Serialize();
			writer.Write(segmentData.Length);
			writer.Write(segmentData);
		}

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	public override void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		int numSegments = reader.ReadInt32();

		for (int i = 0; i < numSegments; ++i)
		{
			int lengthSegment = reader.ReadInt32();
			byte[] segmentData = reader.ReadBytes(lengthSegment);
			ObjectStatePacketBodySegment segment = new ObjectStatePacketBodySegment();
			segment.Deserialize(segmentData);
			segments.Add(segment);
		}

		stream.Close();
	}
}