using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TurretController : MonoBehaviour
{
	[SerializeField] float rotationSpeed = 5f;

    Vector2 aim;
	public Vector2 Aim { set => aim = value; }

	private bool isAiming;
    public bool IsAiming { get => isAiming; set => isAiming = value; }

    [Header("Shooting Variables")]
    [SerializeField] private GameObject firePoint;
    [SerializeField] private float bulletForce;
    [SerializeField] private float bulletSpread;
    [SerializeField] private float bulletChargingTime;
    [SerializeField] private float bulletCooldownTime;
    [SerializeField] private Slider bulletSlider;
    [SerializeField] private Image bulletSliderFill;
    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float shootScreenShakeForce;
    [SerializeField] private float shootScreenShakeLength;
    [SerializeField] private float chargingScreenShakeForce;
    private float bulletChargingTimer;
    private float bulletCooldownTimer;
    private bool isCharging;

    private void Update()
    {
        if (!isAiming)
        {
            isCharging = false;
            bulletChargingTimer = 0;
        }
        HandleTimers();
        BulletSliderController();
    }

    /// <summary>
    /// Handles timers
    /// </summary>
    private void HandleTimers()
    {
        //Cooldown Timer
        if (bulletCooldownTimer > 0)
        {
            bulletCooldownTimer -= Time.deltaTime;
        }

        //Charged timer
        if (isCharging)
        {
            if (bulletChargingTimer < bulletChargingTime)
            {
                ScreenShakeController.Instance.StartShake(bulletChargingTime, Time.deltaTime);
                bulletChargingTimer += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Sets slider color and values based on state of charged attack
    /// </summary>
    private void BulletSliderController()
    {
        if (isCharging)
        {
            bulletSlider.value = (bulletChargingTimer / bulletChargingTime);
            if (bulletChargingTimer < bulletChargingTime)
            {
                //set color to red (temp charging color)
                bulletSliderFill.color = Color.red;
            }
            else if (bulletChargingTimer >= bulletChargingTime)
            {
                //set color is green (temp ability available color)
                bulletSliderFill.color = Color.green;
            }
        }
        else
        {
            if (bulletCooldownTimer > 0)
            {
                bulletSlider.value = (bulletCooldownTimer / bulletCooldownTime);
                //set color to blue (temp cooldown color)
                bulletSliderFill.color = Color.blue;
            }
            else
            {
                bulletSlider.value = 0;
                //set color to red (temp charging color)
                bulletSliderFill.color = Color.red;
            }
        }
    }

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

    /// <summary>
    /// Gets called on shoot attack input and checks if player is holding or not and either starts charging or shoots simple bullet
    /// </summary>
    /// <param name="context"></param>
    public void ShootAttackInput(InputAction.CallbackContext context)
    {
        if (isAiming)
        {
            if (context.performed)
            {
                if(bulletCooldownTimer <= 0)
                {
                    isCharging = true;
                }
            }
            else if (context.canceled)
            {
                if (isCharging)
                {
                    if (bulletChargingTimer >= bulletChargingTime)
                    {
                        //If player has charged up attack
                        BulletAttack(bulletPool.GetPooledObject(true), bulletSpread, bulletForce);
                        ScreenShakeController.Instance.StartShake(shootScreenShakeLength, shootScreenShakeForce);
                        //AudioManager.Instance.StartSound("ChargeShoot");
                        bulletCooldownTimer = bulletCooldownTime;
                    }
                    isCharging = false;
                    bulletChargingTimer = 0;
                }
            }
        }
    }

    /// <summary>
    /// Activates bullet on firepoint position and sets rotation to spread, then addsforce
    /// </summary>
    private void BulletAttack(GameObject bulletObject, float bulletSpread, float bulletForce)
    {
        bulletObject.transform.position = firePoint.transform.position;
        bulletObject.transform.rotation = firePoint.transform.rotation;

        var bulletRigidbody = bulletObject.GetComponent<Rigidbody>();

        float spreadAngle = Random.Range(-bulletSpread, bulletSpread);

        // Take the random angle variation and add it to the initial
        // desiredDirection (which we convert into another angle), which in this case is the players aiming direction
        var x = firePoint.transform.position.x - transform.position.x;
        var z = firePoint.transform.position.z - transform.position.z;
        float rotateAngle = spreadAngle + (Mathf.Atan2(z, x) * Mathf.Rad2Deg);

        // Calculate the new direction we will move in which takes into account 
        // the random angle generated
        var MovementDirection = new Vector3(Mathf.Cos(rotateAngle * Mathf.Deg2Rad), 0, Mathf.Sin(rotateAngle * Mathf.Deg2Rad)).normalized;

        bulletRigidbody.velocity = MovementDirection * bulletForce;
    }
}