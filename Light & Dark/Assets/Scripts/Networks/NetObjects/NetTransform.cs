using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Jobs;

public struct TransformData
{
	public Vector3 position;
	public Vector3 rotation;
	public Vector3 scale;

	public byte[] Serialize()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(position.x);
		writer.Write(position.y);
		writer.Write(position.z);
		writer.Write(rotation.x);
		writer.Write(rotation.y);
		writer.Write(rotation.z);
		writer.Write(scale.x);
		writer.Write(scale.y);
		writer.Write(scale.z);

		byte[] objectAsBytes = stream.ToArray();
		stream.Close();

		return objectAsBytes;
	}
	public void Deserialize(byte[] data)
	{
		Stream stream = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(stream);
		stream.Seek(0, SeekOrigin.Begin);

		float positionX, positionY, positionZ;
		positionX = reader.ReadSingle();
		positionY = reader.ReadSingle();
		positionZ = reader.ReadSingle();
		position = new Vector3(positionX, positionY, positionZ);

		float rotationX, rotationY, rotationZ;
		rotationX = reader.ReadSingle();
		rotationY = reader.ReadSingle();
		rotationZ = reader.ReadSingle();
		rotation = new Vector3(rotationX, rotationY, rotationZ);

		float scaleX, scaleY, scaleZ;
		scaleX = reader.ReadSingle();
		scaleY = reader.ReadSingle();
		scaleZ = reader.ReadSingle();
		scale = new Vector3(scaleX, scaleY, scaleZ);

		stream.Close();
	}
}

public class NetTransform : NetObject
{
	// where each component of the transform is applied
	[SerializeField] Transform netPositionTarget;
	[SerializeField] Transform netRotationTarget;
	[SerializeField] Transform netScaleTarget;

	Vector3 previousPosition;
	Quaternion previousRotation;
	Vector3 previousScale;

	// for interpolation
	Vector3 nextPosition;
	Quaternion nextRotation;
	Vector3 nextScale;

	float timeSinceLastStateChange;

	void Awake()
	{
		netClass = NetObjectClass.TRANSFORM;

		previousPosition = nextPosition = netPositionTarget.position;
		previousRotation = nextRotation = netRotationTarget.rotation;
		previousScale = nextScale = netScaleTarget.localScale;
	}

	void Update()
	{
		timeSinceLastStateChange += Time.deltaTime;

		if (IsOwner())
		{
			bool sendPlayerData = false;
			if (previousPosition != netPositionTarget.position)
				sendPlayerData = true;

			if (previousRotation != netRotationTarget.rotation)
				sendPlayerData = true;

			if (previousScale != netScaleTarget.localScale)
				sendPlayerData = true;

			if (sendPlayerData)
			{
				ObjectStateSegment objectStateSegment = new ObjectStateSegment(ObjectReplicationAction.UPDATE, GetDataToUpdate().Serialize());
				NetObjectsManager.instance.ReceiveObjectStateToSend(netID, objectStateSegment);
			}
		}
		else
		{
			#region INTERPOLATION
			if (previousPosition != nextPosition)
				netPositionTarget.position = Vector3.Lerp(previousPosition, nextPosition, timeSinceLastStateChange);

			if (previousRotation != nextRotation)
				netRotationTarget.rotation = Quaternion.Lerp(previousRotation, nextRotation, timeSinceLastStateChange);

			if (previousScale != nextScale)
				netScaleTarget.localScale = Vector3.Lerp(previousScale, nextScale, timeSinceLastStateChange);
			#endregion
		}
	}

	public override void UpdateObjectData(byte[] objectDataToUpdate)
	{
		TransformData transformData = new TransformData();

		transformData.Deserialize(objectDataToUpdate);

		UpdateNetTransform(transformData);
	}

	void UpdateNetTransform(TransformData transformData)
	{
		timeSinceLastStateChange = 0;

		previousPosition = nextPosition;
		previousRotation = nextRotation;
		previousScale = nextScale;

		nextPosition = transformData.position;
		nextRotation = Quaternion.Euler(transformData.rotation);
		nextScale = transformData.scale;
	}

	public TransformData GetTransformData()
	{
		TransformData transformData = new TransformData();

		transformData.position = netPositionTarget.position;
		transformData.rotation = netRotationTarget.rotation.eulerAngles;
		transformData.scale = netScaleTarget.localScale;

		return transformData;
	}

	protected override byte[] GetObjectData()
	{
		return GetTransformData().Serialize();
	}
}
