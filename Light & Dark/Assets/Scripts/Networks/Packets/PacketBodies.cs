using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

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

public struct ObjectStateSegment
{
	public ObjectStateSegment(ObjectReplicationAction action, int netID, NetObjectClass objectClass, byte[] objectData)
	{
		this.action = action;
		this.netID = netID;
		this.objectClass = objectClass;
		this.objectData = objectData;
	}

	public ObjectReplicationAction action { get; private set; }
	public int netID { get; private set; }
	public NetObjectClass objectClass { get; private set; }
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
		objectClass = (NetObjectClass)reader.ReadInt32();

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

	public List<ObjectStateSegment> segments = new List<ObjectStateSegment>();

	public void AddSegment(ObjectReplicationAction action, int netID, NetObjectClass objectClass, byte[] data)
	{
		segments.Add(new ObjectStateSegment(action, netID, objectClass, data));
	}
	public void AddSegment(ObjectStateSegment segment)
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
			ObjectStateSegment segment = new ObjectStateSegment();
			segment.Deserialize(segmentData);
			segments.Add(segment);
		}

		stream.Close();
	}
}

// -----------------------------------------------------------------------------------
public struct DataToRecreateNetObject
{
	public int netID;
	public NetObjectClass netClass;
	public byte[] data;

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(netID);
		writer.Write((int)netClass);
		writer.Write(data.Length);
		writer.Write(data);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	public void Deserialize(byte[] dataToDeserialize)
	{
		Stream stream = new MemoryStream(dataToDeserialize);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		netID = reader.ReadInt32();
		netClass = (NetObjectClass)reader.ReadInt32();
		int dataLength = reader.ReadInt32();
		data = reader.ReadBytes(dataLength);

		stream.Close();
	}

}

public class LevelReplicationPacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public LevelReplicationPacketBody(string levelName, List<DataToRecreateNetObject> netObjectsData)
	{
		this.levelName = levelName;
		this.netObjectsData = netObjectsData;
	}
	public LevelReplicationPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public string levelName;
	public List<DataToRecreateNetObject> netObjectsData;

	public override byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(levelName);

		writer.Write(netObjectsData.Count);
		foreach (var objectData in netObjectsData)
		{
			byte[] objectBytes = objectData.Serialize();
			writer.Write(objectBytes.Length);
			writer.Write(objectBytes);
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

		levelName = reader.ReadString();

		int nObjects = reader.ReadInt32();

		for (int i = 0; i < nObjects; ++i)
		{
			int lengthItem = reader.ReadInt32();
			byte[] itemData = reader.ReadBytes(lengthItem);
			DataToRecreateNetObject item = new DataToRecreateNetObject();
			item.Deserialize(itemData);
			netObjectsData.Add(item);
		}

		stream.Close();
	}
}