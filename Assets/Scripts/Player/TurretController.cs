using UnityEngine;
using UnityEngine.InputSystem;

public class TurretController : MonoBehaviour
{
	[SerializeField] float rotationSpeed = 5f;

	Vector2 aimVector;

	void Awake()
	{
		inputActions = new PlayerInput();
	}

	private void FixedUpdate()
	{
		aimVector = aim.ReadValue<Vector2>(); // keep the same worldRotation when hull is rotated


		if (aimVector != Vector2.zero)
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new(aimVector.x, 0, aimVector.y)), rotationSpeed);
	}

	#region InputSystem
	PlayerInput inputActions;
	InputAction aim;

	void OnEnable()
	{
		aim = inputActions.Player.Aim;
		aim.Enable();
	}

	void OnDisable()
	{
		aim.Disable();
	}
	#endregion InputSystem
}