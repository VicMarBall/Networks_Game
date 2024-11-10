using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetObject))]
public class NetTransform : MonoBehaviour
{
    NetObject netObject;

    Vector3 previousPosition;
    Quaternion previousRotation;
    Vector3 previousLocalScale;

	private void Awake()
	{
		netObject = GetComponent<NetObject>();
	}

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
        if (transform.position != previousPosition) {  }
        if (transform.rotation != previousRotation) {  }
        if (transform.localScale != previousLocalScale) {  }
	}
}
