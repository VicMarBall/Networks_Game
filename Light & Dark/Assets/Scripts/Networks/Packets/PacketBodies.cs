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
	public ObjectStateSegment(ObjectReplicationAction action, byte[] dataToAction)
	{
		this.action = action;
		this.dataToAction = dataToAction;
	}

	public ObjectReplicationAction action { get; private set; }
	public byte[] dataToAction { get; private set; }

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((int)action);
		// maybe we can take out this one and calculate at deserialization
		writer.Write(dataToAction.Length);
		writer.Write(dataToAction);

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
		int dataToActionLength = reader.ReadInt32();
		dataToAction = reader.ReadBytes(dataToActionLength);

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

	public void AddSegment(ObjectReplicationAction action, byte[] actionData)
	{
		segments.Add(new ObjectStateSegment(action, actionData));
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