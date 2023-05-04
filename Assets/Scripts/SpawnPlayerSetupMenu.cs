using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Gets instantiated on player joined and spawns setup menu on horizontal layout group. Sets up player index.
/// </summary>
public class SpawnPlayerSetupMenu : MonoBehaviour
{
    [SerializeField] private GameObject playerSetupMenuPrefab;
    [SerializeField] private PlayerInput playerInput;

   private void Awake()
    {
        var rootMenu = GameObject.FindObjectOfType<HorizontalLayoutGroup>();
        if(rootMenu != null)
        {
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
            playerInput.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(playerInput.playerIndex);
        }
    }

}
