using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Initilizes players on chosen positions and calls function to set up their inputs etc.
/// </summary>
public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private List<Vector3> playerSpawnPositions = new List<Vector3>();
    [SerializeField] private GameObject playerPrefab;

    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var player = Instantiate(playerPrefab, playerSpawnPositions[i], Quaternion.identity);
            player.GetComponent<PlayerInputHandler>().InitializePlayer(playerConfigs[i]);
        }
    }
}
