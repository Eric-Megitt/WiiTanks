using UnityEngine;

using BehaviourTree;
using System;

public class EnemyMovementBT : BehaviourTree.Tree
{
	[SerializeField] float forwardWalldetect;

	[SerializeField] Vector2 targetVector;

	[SerializeField] float moveSpeed = 5f;
	[SerializeField] float rotationSpeed = 5f;

	Vector3 TargetPos => FindObjectOfType<PlayerMovement>().transform.position;



	protected override Node SetupTree()
	{
		//targetPos = new Vector3(transform.position.x + targetVector.x, 0, transform.position.z + targetVector.y);

		Rigidbody _rigidbody = GetComponent<Rigidbody>();

		return
			new Sequence
			(
				new RaycastNode // GroundCheck
				(
					() => transform.position,
					() => Vector3.down,
					() => .1f + (GetComponent<BoxCollider>().size.y / 2)
				),

				new LambdaNode(() => !Physics.Raycast(transform.position, transform.rotation * Vector3.forward, forwardWalldetect)),

				new DriveToPos(transform, _rigidbody, () => TargetPos, moveSpeed, rotationSpeed)
			); ;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + (transform.forward * forwardWalldetect));
	}
}

public class DriveToPos : Node
{
	Transform transform;
	Rigidbody rigidbody;

	Func<Vector3> targetPos;
	float speed;
	float rotationSpeed;

	Vector3 moveVector;
	bool canMove;

    public DriveToPos(Transform transform, Rigidbody rigidbody, Func<Vector3> targetPos, float speed, float rotationSpeed)
    {
        this.transform = transform;
		this.rigidbody = rigidbody;

		this.targetPos = targetPos;
		this.speed = speed;
		this.rotationSpeed = rotationSpeed;
    }

	protected override NodeState Evaluate()
	{
		GetMoveVector();
		Move();
		return NodeState.SUCCESS;
	}
	void Move()
	{
		Rotate();
		if (!canMove)
			return;

		rigidbody.velocity = new(moveVector.x, rigidbody.velocity.y, moveVector.z);
	}

	void GetMoveVector()
	{
		moveVector = targetPos.Invoke() - new Vector3(transform.position.x, 0, transform.position.z);


		moveVector = InterCardinal(moveVector);
		moveVector *= speed;

	}

	Vector3 InterCardinal(Vector3 ve)
	{
		if (ve.magnitude < .5f)
		{
			Debug.Log($"{ve} -> {Vector3.zero}");
			return Vector3.zero;
		}


		Vector3 returnVector = Vector3.zero;
		Vector3 v = ve.normalized;

		float sinSixteenthTurnRad = Mathf.Sin(Mathf.PI / 8);

		if (Mathf.Abs(v.x) > sinSixteenthTurnRad)
		{
			returnVector += new Vector3(Mathf.Sign(v.x), 0, 0);
		}
		if (Mathf.Abs(v.z) > sinSixteenthTurnRad)
		{
			returnVector += new Vector3(0, 0, Mathf.Sign(v.z));
		}

		Debug.Log($"{ve} -> {returnVector.normalized}");

		return returnVector.normalized;
	}

	void Rotate()
	{
		if (moveVector != Vector3.zero)
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveVector), rotationSpeed);

		canMove = transform.rotation == Quaternion.LookRotation(moveVector);
	}

}

class RaycastNode : Node
{
	readonly Func<Vector3> origin;
	readonly Func<Vector3> target;
	readonly Func<float> distance = null;

	Vector3 Origin => origin.Invoke();
	Vector3 Target => target.Invoke();
	float Distance => (distance == null) ? (target.Invoke() - origin.Invoke()).magnitude : distance.Invoke();

	#region Ctors
	public RaycastNode(Func<Vector3> origin, Func<Vector3> target)
	{
		this.origin = origin;
		this.target = target;
	}

	public RaycastNode(Func<Vector3> origin, Func<Vector3> direction, Func<float> distance)
	{
		this.origin = origin;
		this.target = direction;
		this.distance = distance;
	}
	#endregion Ctors

	protected override NodeState Evaluate()
	{
		return Physics.Raycast(Origin, Target, Distance) ? NodeState.SUCCESS : NodeState.FAILURE;
	}
}