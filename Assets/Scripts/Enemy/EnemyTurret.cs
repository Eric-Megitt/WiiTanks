using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyTurret: MonoBehaviour
{
	[SerializeField] float rotationSpeed = 5f;

	Vector2 aimVector = Vector2.zero;
	Vector3 AimVector => FindObjectOfType<PlayerMovement>().transform.position - transform.position;
	Quaternion AimQuad => Quaternion.LookRotation(new(AimVector.x, 0, AimVector.z));

	private void FixedUpdate()
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation, AimQuad, rotationSpeed);
	}

}