using UnityEngine;
using UnityEngine.InputSystem;

public class TurretController : MonoBehaviour
{
	[SerializeField] float rotationSpeed = 5f;

    Vector2 aim;
	public Vector2 Aim { set => aim = value; }

	private bool isAiming;
    public bool IsAiming { get => isAiming; set => isAiming = value; }

	private void FixedUpdate()
	{
		Rotate();
	}

	private void Rotate()
	{
        if (!isAiming)
            return;

        if (aim != Vector2.zero)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new(aim.x, 0, aim.y)), rotationSpeed);
    }
}