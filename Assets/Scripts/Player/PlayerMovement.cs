
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using ScottsEssentials;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] float moveSpeed = 5f;
	[SerializeField, Tooltip("Degrees the tank rotates per FixedUpdate")] float rotationSpeed = 5f;
	[Header("Acceleration"), Space(-5)]
	[SerializeField] AnimationCurve accelerationCurve = AnimationCurve.Linear(0,0,1,1);
	[SerializeField, Tooltip("Milliseconds")] int accelerationTime = 0;
	int accelerationTimer = 0;

	bool canMove = true;

	private bool isMoving;
	public bool IsMoving { get => isMoving; set => isMoving = value; }

    private Vector2 movement;
	public Vector2 Movement { set => movement = value; }

    Rigidbody _rigidbody;

	Vector3 moveVector;
	TurretController turretController;

	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
        turretController = GetComponentInChildren<TurretController>();
    }

	private void FixedUpdate()
	{
		GetMoveVector();
		Move(Time.fixedDeltaTime);
        if (!isMoving)
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }

	void Move(float timeSinceLastCalled)
	{
		Rotate();

		if (!canMove)
			return;

        if (!isMoving)
		{
			_rigidbody.velocity = Vector3.zero;
            return;
        }

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
        if (turretController.IsAiming)
            return;

        if (moveVector != Vector3.zero)
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveVector), rotationSpeed);

		canMove = transform.rotation == Quaternion.LookRotation(moveVector);
	}

	void GetMoveVector()
	{
		moveVector = movement * moveSpeed;
		moveVector = new(moveVector.x, 0f, moveVector.y);
	}
}