using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds functions for player setup menu.
/// </summary>
public class PlayerSetupMenuController : MonoBehaviour
{
	[Header("Player Setup Menu References")]
	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private GameObject readyPanel;
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private Button readyButton;

	private int playerIndex;

	private float ignoreInputTime = 1.5f;
	private bool inputEnabled;

	/// <summary>
	/// Sets player index.
	/// </summary>
	/// <param name="pi"></param>
	public void SetPlayerIndex(int pi)
	{
		playerIndex = pi;
		titleText.text = "Player " + (pi + 1);
		ignoreInputTime = Time.time + ignoreInputTime;	//wait time from join to being able to interact with UI.
	}

	void Update()
	{
		if(Time.time > ignoreInputTime)
		{
			inputEnabled = true;
		}
	}

    /// <summary>
    /// Sets player sprite depending on choice from UI button, then moves on to ready panel.
    /// </summary>
    /// <param name="pi"></param>
    public void SetColor(int colorIndex)
	{
		if (!inputEnabled) {return;}

		Color color = Color.white;

		switch (colorIndex)
		{
			case 0:
				color = Color.red;
				break;
			case 1:
                color = Color.green;
                break;
			case 2:
                color = Color.blue;
                break;
			case 3:
                color = Color.magenta;
                break;
			default:
				break;
		}

		PlayerConfigurationManager.instance.SetPlayerColor(playerIndex, color);
		readyPanel.SetActive(true);
		readyButton.Select();
		menuPanel.SetActive(false);
	}

	/// <summary>
	/// Sets playerConfiguration bool as ready.
	/// </summary>
	public void ReadyPlayer()
	{
        if (!inputEnabled) { return; }

		PlayerConfigurationManager.instance.ReadyPlayer(playerIndex);
		readyButton.gameObject.SetActive(false);
    }
}
