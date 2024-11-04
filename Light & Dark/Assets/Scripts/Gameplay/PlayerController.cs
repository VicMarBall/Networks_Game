using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Rigidbody rigidBody;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckMovement();
    }

    void CheckMovement()
    {
        Vector3 velocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            velocity += transform.rotation * Vector3.forward;
        }
		if (Input.GetKey(KeyCode.A))
		{
			velocity += transform.rotation * Vector3.left;
		}
		if (Input.GetKey(KeyCode.S))
		{
			velocity += transform.rotation * Vector3.back;
		}
		if (Input.GetKey(KeyCode.D))
		{
			velocity += transform.rotation * Vector3.right;
		}

		velocity.Normalize();

        if (velocity != Vector3.zero)
        {
			rigidBody.velocity = velocity;
		}
	}
}
