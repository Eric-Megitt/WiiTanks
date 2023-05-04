using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.InputSystem.InputAction;

/// <summary>
/// Handles player input and calls functions accordingly.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private float controllerDeadzone = 0.1f;

    private PlayerMovement playerMovementReference;
    private TurretController turretControllerReference;
    private PlayerConfiguration playerConfig;

    private PlayerControls playerControls;
    private bool state;

    private void Awake()
    {
        playerMovementReference = GetComponent<PlayerMovement>();
        turretControllerReference = GetComponentInChildren<TurretController>();
        playerControls = new PlayerControls();
    }

    /// <summary>
    /// Sets up player and assigns color, sprite playerConfiguration.
    /// </summary>
    /// <param name="pc"></param>
    public void InitializePlayer(PlayerConfiguration pc)
    {
        playerConfig = pc;
        Material material = GetComponent<Renderer>().material;
        material.SetColor("_EmissionColor", pc.PlayerColor);
        playerConfig.Input.onActionTriggered += Input_onActionTriggered;
        playerControls.PlayerMovement.Enable();
    }

    private void OnDisable()
    {
        playerConfig.Input.onActionTriggered -= Input_onActionTriggered;
        playerControls.PlayerMovement.Disable();
    }

    public int GetPlayerIndex()
    {
        return playerConfig.PlayerIndex;
    }

    /// <summary>
    /// On inputFunction checks input and if it was performed by current instance of player, and if so calls move function
    /// </summary>
    /// <param name="context"></param>
    private void Input_onActionTriggered(CallbackContext context)
    {
        if(context.action.name == playerControls.PlayerMovement.Movement.name)
        {
            OnMove(context);
        }

        if (context.action.name == playerControls.PlayerMovement.Shoot.name)
        {
            if (context.performed || context.canceled)
            {
                OnShoot(context);
            }
        }

        if(context.action.name == playerControls.PlayerMovement.StateChange.name)
        {
            if (context.performed)
            {
                ChangeState(context);
            }
        }
    }

    /// <summary>
    /// Gets player movement input and sends it to player.
    /// </summary>
    /// <param name="context"></param>
    public void OnMove(CallbackContext context)
    {
        if (playerMovementReference != null)
        {
            if (state)
            {
                playerMovementReference.Movement = context.ReadValue<Vector2>().normalized;
            }
            else
            {
                turretControllerReference.Aim = context.ReadValue<Vector2>().normalized;
            }
        }
    }

    public void OnShoot(CallbackContext context)
    {
        if (playerMovementReference != null)
        {
            if (state)
            {
                if (context.performed)
                {
                    playerMovementReference.IsMoving = true;
                }
                else
                {
                    playerMovementReference.IsMoving = false;
                }
            }
        }
    }

    public void ChangeState(CallbackContext context)
    {
        state = !state;
        if (!state)
        {
            playerMovementReference.IsMoving = false;
            turretControllerReference.IsAiming = true;
        }
        else
        {
            turretControllerReference.IsAiming = false;
        }
    }
}
