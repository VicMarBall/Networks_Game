using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public struct PlayerData
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

public class NetPlayer : NetObject
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
        netClass = NetObjectClass.PLAYER;
		
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

			float normalizedTime = timeSinceLastStateChange / NetObjectsManager.instance.packetPeriod;

			if (previousPosition != nextPosition)
				netPositionTarget.position = Vector3.Lerp(previousPosition, nextPosition, normalizedTime);

			if (previousRotation != nextRotation)
				netRotationTarget.rotation = Quaternion.Lerp(previousRotation, nextRotation, normalizedTime);

			if (previousScale != nextScale)
				netScaleTarget.localScale = Vector3.Lerp(previousScale, nextScale, normalizedTime);
			#endregion
		}
	}

	public override void InitializeObjectData(byte[] objectDataToInitialize)
	{
		PlayerData playerData = new PlayerData();

		playerData.Deserialize(objectDataToInitialize);

		previousPosition = nextPosition = playerData.position;
		previousRotation = nextRotation = Quaternion.Euler(playerData.rotation);
		previousScale = nextScale = playerData.scale;

		if (!IsOwner())
		{
			netPositionTarget.position = playerData.position;
			netRotationTarget.rotation = Quaternion.Euler(playerData.rotation);
			netScaleTarget.localScale = playerData.scale;
		}
	}

	public override void UpdateObjectData(byte[] objectDataToUpdate)
	{
		PlayerData playerData = new PlayerData();

		playerData.Deserialize(objectDataToUpdate);

		UpdateNetPlayer(playerData);
	}

	void UpdateNetPlayer(PlayerData playerData)
    {
		timeSinceLastStateChange = 0;

		previousPosition = nextPosition;
        previousRotation = nextRotation;
        previousScale = nextScale;

        nextPosition = playerData.position;
		nextRotation = Quaternion.Euler(playerData.rotation);
		nextScale = playerData.scale;
	}

	public PlayerData GetPlayerData()
	{
		PlayerData playerData = new PlayerData();

		playerData.position = netPositionTarget.position;
		playerData.rotation = netRotationTarget.rotation.eulerAngles;
		playerData.scale = netScaleTarget.localScale;

		return playerData;
	}

	protected override byte[] GetObjectData()
	{
		return GetPlayerData().Serialize();
	}
}
