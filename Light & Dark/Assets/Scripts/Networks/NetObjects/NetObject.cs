using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct DataToCreateNetObject
{
	public int ownerID;
	public NetObjectClass netClass;
	public byte[] objectData;

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(ownerID);
		writer.Write((int)netClass);
		writer.Write(objectData.Length);
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

		ownerID = reader.ReadInt32();
		netClass = (NetObjectClass)reader.ReadInt32();
		int dataLength = reader.ReadInt32();
		objectData = reader.ReadBytes(dataLength);

		stream.Close();
	}
}

public struct DataToRecreateNetObject
{
	public int ownerID;
	public int netID;
	public NetObjectClass netClass;
	public byte[] objectData;

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(ownerID);
		writer.Write(netID);
		writer.Write((int)netClass);
		writer.Write(objectData.Length);
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

		ownerID = reader.ReadInt32();
		netID = reader.ReadInt32();
		netClass = (NetObjectClass)reader.ReadInt32();
		int dataLength = reader.ReadInt32();
		objectData = reader.ReadBytes(dataLength);

		stream.Close();
	}
}

public struct DataToUpdateNetObject
{
	public int netID;
	public byte[] objectData;

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(netID);
		writer.Write(objectData.Length);
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

		netID = reader.ReadInt32();
		int dataLength = reader.ReadInt32();
		objectData = reader.ReadBytes(dataLength);

		stream.Close();
	}
}

public struct DataToDestroyNetObject
{
	public int netID;

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(netID);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		netID = reader.ReadInt32();

		stream.Close();
	}
}

public abstract class NetObject : MonoBehaviour
{
	public int ownerID;
	public int netID = -1;
	public NetObjectClass netClass { get; protected set; }

	public bool IsOwner() { return (ownerID == NetworkingEnd.instance.userID); }

	public abstract void InitializeObjectData(byte[] dataToInitialize);
	public abstract void UpdateObjectData(byte[] dataToUpdate);

	public DataToCreateNetObject GetDataToCreate()
	{
		DataToCreateNetObject dataToCreate = new DataToCreateNetObject();

		dataToCreate.ownerID = ownerID;
		dataToCreate.netClass = netClass;
		dataToCreate.objectData = GetObjectData();

		return dataToCreate;
	}
	public DataToRecreateNetObject GetDataToRecreate()
	{
		DataToRecreateNetObject dataToRecreate = new DataToRecreateNetObject();

		dataToRecreate.ownerID = ownerID;
		dataToRecreate.netID = netID;
		dataToRecreate.netClass = netClass;
		dataToRecreate.objectData = GetObjectData();

		return dataToRecreate;
	}
	public DataToUpdateNetObject GetDataToUpdate()
	{
		DataToUpdateNetObject dataToUpdate = new DataToUpdateNetObject();

		dataToUpdate.netID = netID;
		dataToUpdate.objectData = GetObjectData();

		return dataToUpdate;
	}
	public DataToDestroyNetObject GetDataToDestroy()
	{
		DataToDestroyNetObject dataToDestroy = new DataToDestroyNetObject();

		dataToDestroy.netID = netID;

		return dataToDestroy;
	}

	protected abstract byte[] GetObjectData();
}