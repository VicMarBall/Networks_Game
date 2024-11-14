using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// TO IMPLEMENT probably for this monday only a player replication (maybe should be the transform, maybe the velocity, not sure)

public enum ObjectReplicationClass
{
	POSITION,
	ROTATION,
	SCALE,
	TRANSFORM,
	LOCAL_PLAYER,
	FOREIGN_PLAYER
}

public static class ObjectReplicationRegistry
{
	// Vector3
	public static byte[] SerializeVector3(Vector3 vector3)
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(vector3.x);
		writer.Write(vector3.y);
		writer.Write(vector3.z);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;

	}
	public static Vector3 DeserializeVector3(byte[] bytes)
	{
		MemoryStream stream = new MemoryStream(bytes);
		BinaryReader reader = new BinaryReader(stream);

		Vector3 vector3 = new Vector3();

		vector3.x = reader.ReadSingle();
		vector3.y = reader.ReadSingle();
		vector3.z = reader.ReadSingle();

		stream.Close();

		return vector3;
	}

	// Quaternion
	public static byte[] SerializeQuaternion(Quaternion quaternion)
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(quaternion.x);
		writer.Write(quaternion.y);
		writer.Write(quaternion.z);
		writer.Write(quaternion.w);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;

	}
	public static Quaternion DeserializeQuaternion(byte[] bytes)
	{
		MemoryStream stream = new MemoryStream(bytes);
		BinaryReader reader = new BinaryReader(stream);

		Quaternion quaternion = new Quaternion();

		quaternion.x = reader.ReadSingle();
		quaternion.y = reader.ReadSingle();
		quaternion.z = reader.ReadSingle();
		quaternion.w = reader.ReadSingle();

		stream.Close();

		return quaternion;
	}

	public static Transform DeserializeTransform()
	{
		Transform ret = null;
		return ret;
	}
}