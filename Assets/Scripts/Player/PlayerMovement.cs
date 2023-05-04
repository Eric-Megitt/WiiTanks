
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using ScottsEssentials;

public class PlayerMovement : Singleton<PlayerMovement>
{
	[Header("Movement")]
	[SerializeField] float moveSpeed = 5f;
	[SerializeField, Tooltip("Degrees the tank rotates per FixedUpdate")] float rotationSpeed = 5f;
	[Header("Acceleration"), Space(-5)]
	[SerializeField] AnimationCurve accelerationCurve = AnimationCurve.Linear(0,0,1,1);
	[SerializeField, Tooltip("Milliseconds")] int accelerationTime = 0;
	int accelerationTimer = 0;

	bool canMove = true;

	Rigidbody _rigidbody;

	Vector3 moveVector;

	protected override void WakeUp()
	{
		inputActions = new PlayerInput();
	}

	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		GetMoveVector();
		Move(Time.fixedDeltaTime);
	}

	void Move(float timeSinceLastCalled)
	{
		Rotate();

		if (!canMove)
			return;

		if (accelerationTimer < accelerationTime)
		{
			_rigidbody.velocity = (new Vector3(moveVector.x, 0, moveVector.z) * accelerationCurve.Evaluate((float)accelerationTimer / (float)accelerationTime)) + new Vector3(0, _rigidbody.velocity.y, 0);
			accelerationTimer = Mathf.Clamp(accelerationTimer + (int)(timeSinceLastCalled * 1000), 0, accelerationTime);
		}
		else
			_rigidbody.velocity = new(moveVector.x, _rigidbody.velocity.y, moveVector.z);
	}

	void Rotate()
	{
		if (moveVector != Vector3.zero)
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveVector), rotationSpeed);

		canMove = transform.rotation == Quaternion.LookRotation(moveVector);
	}

	void GetMoveVector()
	{
		moveVector = movement.ReadValue<Vector2>() * moveSpeed;
		moveVector = new(moveVector.x, 0f, moveVector.y);
	}

	#region InputSystem
	PlayerInput inputActions;
	InputAction movement;

	void OnEnable()
	{
		movement = inputActions.Player.Movement;
		movement.Enable();
	}

	void OnDisable()
	{
		movement.Disable();
	}
	#endregion InputSystem
}