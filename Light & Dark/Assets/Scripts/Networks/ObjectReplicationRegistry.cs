using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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