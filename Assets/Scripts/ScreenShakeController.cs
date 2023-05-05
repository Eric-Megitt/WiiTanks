using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeController : Singleton<ScreenShakeController>
{
    //how to call screen shake
    //ScreenShakeController.instance.StartShake(screenShakeLength, screenShakePower);

    [SerializeField] private float rotationMultiplier;
    [SerializeField] private float returnSpeed;

    private float shakeTimeRemaining, shakePower, shakeFadeTime, shakeRotation;

    private Vector3 originPosition;

    private void Start()
    {
        originPosition = transform.position;
    }

    private void LateUpdate()
    {
        //shake the camera for the shaketime given
        if (shakeTimeRemaining > 0f)
        {
            //Make timer go down
            shakeTimeRemaining -= Time.deltaTime;

            //get random amount of shake on each axis
            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float zAmount = Random.Range(-1f, 1f) * shakePower;

            //transform camera to random taken x and y amounts
            transform.position += new Vector3(xAmount, 0, zAmount);

            //make shake fade
            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);
            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.Lerp(transform.position, originPosition, returnSpeed * Time.deltaTime);
        }

        //rotation shake too cause why not
        transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));
    }

    public void StartShake(float length, float power)
    {
        //setting variables
        shakeTimeRemaining = length;
        shakePower = power;

        shakeFadeTime = power / length;

        shakeRotation = power * rotationMultiplier;
    }
}
