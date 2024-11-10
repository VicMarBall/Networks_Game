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
	PLAYER
}

public static class ObjectReplicationRegistry
{
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
	public static Transform DeserializeTransform()
	{
		Transform ret = null;
		return ret;
	}
}