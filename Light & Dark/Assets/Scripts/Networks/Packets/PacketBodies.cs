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
public class PingPacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public PingPacketBody(float latency)
	{
		this.latency = latency;
	}
	public PingPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public float latency;

	public override byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(latency);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public override void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		latency = reader.ReadSingle();

		stream.Close();
	}
}

// ---------------------------------------------------------------------------
public class PongPacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public PongPacketBody()
	{

	}
	public PongPacketBody(byte[] data)
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

		writer.Write((char)action);
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

		action = (ObjectReplicationAction)reader.ReadChar();
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

public enum RequestType
{
	LEVEL_REPLICATION,
	CREATE_PLAYER
}

public class RequestPacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public RequestPacketBody(RequestType requestType)
	{
		this.requestType = requestType;
		this.additionalData = new byte[0];
	}
	public RequestPacketBody(RequestType requestType, byte[] additionalData)
	{
		this.requestType = requestType;
		this.additionalData = additionalData;
	}
	public RequestPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public RequestType requestType;
	byte[] additionalData;

	public override byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((char)requestType);

		writer.Write(additionalData.Length);
		writer.Write(additionalData);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	public override void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		requestType = (RequestType)reader.ReadChar();

		int additionalDataLength = reader.ReadInt32();
		additionalData = reader.ReadBytes(additionalDataLength);

		stream.Close();
	}
}

// ---------------------------------------------------------------------------
public class ByePacketBody : PacketBody
{
	// constructor to send
	// constructor to recieve
	public ByePacketBody(bool fromServer)
	{
		this.fromServer = fromServer;
	}
	public ByePacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public bool fromServer;

	public override byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(fromServer); 

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public override void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		fromServer = reader.ReadBoolean();
		
		stream.Close();
	}
}