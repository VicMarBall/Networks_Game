using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public enum PacketBodyType
{
	HELLO, // client requests to server entrance
	WELCOME,
	PING, 
	OBJECT_STATE,
	TESTING,
	HARCODED_PLAYER // TO DELETE
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
	public int GetPlayerID() { return playerID; }
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
			case PacketBodyType.WELCOME:
				body = new WelcomePacketBody(reader.ReadBytes(bodyLength));
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

	public override byte[] Serialize() { return new byte[1]; }
	public override void Deserialize(byte[] data) { }
}

public class WelcomePacketBody : PacketBody
{
	// send
	public WelcomePacketBody(int newPlayerID)
	{
		this.newPlayerID = newPlayerID;
	}

	// recieve
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

public class ObjectStatePacketBody : PacketBody
{
	public List<ObjectStatePacketBodySegment> segments = new List<ObjectStatePacketBodySegment>();

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

public class PlayerPacketBody : PacketBody
{
	public ObjectReplicationAction action;
	public Transform transform;

	// send
	public PlayerPacketBody(ObjectReplicationAction action, Transform transform) 
	{
		this.action = action;
		this.transform = transform;
	}

	// recieve
	public PlayerPacketBody(byte[] data)
	{
		Deserialize(data);
	}

	public override byte[] Serialize() 
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write((char)action);

		Matrix4x4 transformMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

		writer.Write(transformMatrix.m00);
		writer.Write(transformMatrix.m01);
		writer.Write(transformMatrix.m02);
		writer.Write(transformMatrix.m03);
		writer.Write(transformMatrix.m10);
		writer.Write(transformMatrix.m11);
		writer.Write(transformMatrix.m12);
		writer.Write(transformMatrix.m13);
		writer.Write(transformMatrix.m20);
		writer.Write(transformMatrix.m21);
		writer.Write(transformMatrix.m22);
		writer.Write(transformMatrix.m23);
		writer.Write(transformMatrix.m30);
		writer.Write(transformMatrix.m31);
		writer.Write(transformMatrix.m32);
		writer.Write(transformMatrix.m33);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}

	public override void Deserialize(byte[] data) 
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		action = (ObjectReplicationAction)reader.ReadChar();

		Matrix4x4 transformationMatrix;

		transformationMatrix.m00 = reader.ReadSingle();
		transformationMatrix.m01 = reader.ReadSingle();
		transformationMatrix.m02 = reader.ReadSingle();
		transformationMatrix.m03 = reader.ReadSingle();
		transformationMatrix.m10 = reader.ReadSingle();
		transformationMatrix.m11 = reader.ReadSingle();
		transformationMatrix.m12 = reader.ReadSingle();
		transformationMatrix.m13 = reader.ReadSingle();
		transformationMatrix.m20 = reader.ReadSingle();
		transformationMatrix.m21 = reader.ReadSingle();
		transformationMatrix.m22 = reader.ReadSingle();
		transformationMatrix.m23 = reader.ReadSingle();
		transformationMatrix.m30 = reader.ReadSingle();
		transformationMatrix.m31 = reader.ReadSingle();
		transformationMatrix.m32 = reader.ReadSingle();
		transformationMatrix.m33 = reader.ReadSingle();

		transform.position = transformationMatrix.GetPosition();
		transform.rotation = transformationMatrix.rotation;
		transform.localScale = transformationMatrix.lossyScale;
	}
}