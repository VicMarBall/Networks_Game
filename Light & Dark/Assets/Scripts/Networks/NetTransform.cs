using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTransform : NetComponent
{
    Vector3 previousPosition;
    Quaternion previousRotation;
    Vector3 previousLocalScale;

	// Start is called before the first frame update
	void Start()
    {
		previousPosition = transform.position;
        previousRotation = transform.rotation;
        previousLocalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
		if (!netObject.playerIsOwner) { return; }

        if (transform.position != previousPosition) 
        {
            byte[] bytes = ObjectReplicationRegistry.SerializeVector3(transform.position);
            ObjectStatePacketBodySegment segment = new ObjectStatePacketBodySegment(ObjectReplicationAction.UPDATE, netObject.netID, ObjectReplicationClass.POSITION, bytes);
            NetObjectsManager.instance.PrepareBodySegment(segment);

			previousPosition = transform.position;
		}
		if (transform.rotation != previousRotation) 
        {
			byte[] bytes = ObjectReplicationRegistry.SerializeQuaternion(transform.rotation);
			ObjectStatePacketBodySegment segment = new ObjectStatePacketBodySegment(ObjectReplicationAction.UPDATE, netObject.netID, ObjectReplicationClass.ROTATION, bytes);
			NetObjectsManager.instance.PrepareBodySegment(segment);

			previousRotation = transform.rotation;
		}
		if (transform.localScale != previousLocalScale) 
        {
			byte[] bytes = ObjectReplicationRegistry.SerializeVector3(transform.localScale);
			ObjectStatePacketBodySegment segment = new ObjectStatePacketBodySegment(ObjectReplicationAction.UPDATE, netObject.netID, ObjectReplicationClass.SCALE, bytes);
			NetObjectsManager.instance.PrepareBodySegment(segment);

			previousLocalScale = transform.localScale;
		}
	}
}
