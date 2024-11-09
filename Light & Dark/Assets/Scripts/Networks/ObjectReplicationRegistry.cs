using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TO IMPLEMENT probably for this monday only a player replication (maybe should be the transform, maybe the velocity, not sure)

public enum ObjectReplicationClass
{
	TRANSFORM
}

public static class ObjectReplicationRegistry
{
	public static Transform DeserializeTransform()
	{
		Transform ret = null;
		return ret;
	}
}