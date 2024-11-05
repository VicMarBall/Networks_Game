using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on Dave / GameDevelopment First Person Movement Tutorial
public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	public float movementSpeed;

	public float groundDrag;

	public float jumpForce;
	public float jumpCooldown;
	public float airMultiplier;
	bool readyToJump = true;

	[Header("Keybinds")]
	public KeyCode jumpKey = KeyCode.Space;

	[Header("Ground Check")]
	public float playerHeight;
	public LayerMask whatIsGround;
	bool grounded;

	public Transform orientation;

	float horizontalInput;
	float verticalInput;

	Vector3 moveDirection;

	Rigidbody rb;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
	}

	void FixedUpdate()
	{
		MovePlayer();
	}

	// Update is called once per frame
	void Update()
	{
		// grounded check
		grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

		GetInputs();
		LimitSpeed();

		// handle drag
		if (grounded) {
			rb.drag = groundDrag;
		}
		else {
			rb.drag = 0;
		}
	}

	void GetInputs()
	{
		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");

		// jump
		if (Input.GetKey(jumpKey) && readyToJump && grounded)
		{
			readyToJump = false;
			Jump();

			Invoke(nameof(ResetJump), jumpCooldown);
		}
	}

	void MovePlayer()
	{
		// calculate movement direction
		moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

		if (grounded) {
			rb.AddForce(moveDirection.normalized * movementSpeed * 10f, ForceMode.Force);
		}
		else {
			rb.AddForce(moveDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);
		}

	}

	void LimitSpeed()
	{
		Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

		// limit velocity if needed
		if (flatVelocity.magnitude > movementSpeed)
		{
			Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
			rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
		}
	}

	void Jump()
	{
		// reset y velocity
		rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

		rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
	}

	void ResetJump()
	{
		readyToJump = true;
	}
}
