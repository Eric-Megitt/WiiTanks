using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerConfigurationManager : MonoBehaviour
{
    [SerializeField] private int maxPlayers;

    private List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();

    public static PlayerConfigurationManager instance;
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
        playerConfigs = new List<PlayerConfiguration>();
    }

    /// <summary>
    /// Returns player configurations
    /// </summary>
    /// <returns></returns>
    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    /// <summary>
    /// Sets playerConfiguration's color
    /// </summary>
    /// <param name="index"></param>
    /// <param name="color"></param>
    public void SetPlayerColor(int index, Color color)
    {
        playerConfigs[index].PlayerColor = color;
    }

    /// <summary>
    /// Sets playerConfiguration ready bool to true, if everyone is ready it loads scene.
    /// </summary>
    /// <param name="index"></param>
    public void ReadyPlayer(int index)
    {
        //setting player as ready
        playerConfigs[index].IsReady = true;
        //if player count is max and all players are ready
        if(playerConfigs.Count == maxPlayers && playerConfigs.All(p => p.IsReady == true))
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    /// <summary>
    /// Handler on player join, adds new player configuration.
    /// </summary>
    /// <param name="pi"></param>
    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player Joined" + pi.playerIndex);
        //checking index to make sure player has not already been added
        if(!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            //configuring new player and adding its inputManager as a child of PlayerConfigurationManager(this)
            pi.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(pi));
        }
    }

}

/// <summary>
/// Holds information for each player regarding color, sprite, PlayerInput etc.
/// </summary>
/// <param name="pi"></param>
public class PlayerConfiguration
{

    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }
    public PlayerInput Input { get; set; }

    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    public Color PlayerColor { get; set; }
}