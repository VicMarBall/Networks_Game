using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on Dave / GameDevelopment Third Person Movement Tutorial
public class ThirdPersonCamera : MonoBehaviour
{
	[Header("References")]
	public Transform orientation;
	public Transform player;
	public Transform playerObj;
	public Rigidbody rigidBody;

	public float rotationSpeed;

	// Start is called before the first frame update
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Update is called once per frame
	void Update()
	{
		// rotate orientation
		Vector3 viewDirection = player.position - new Vector3 (transform.position.x, player.position.y, transform.position.z);
		orientation.forward = viewDirection.normalized;

		// rotate player object
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");
		Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

		if (inputDir != Vector3.zero) {
			playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, rotationSpeed * Time.deltaTime);
		}
	}
}
