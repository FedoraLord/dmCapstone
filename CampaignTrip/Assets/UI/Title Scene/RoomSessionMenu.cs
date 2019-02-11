using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoomSessionMenu : NavigationMenu
{
    [HideInInspector] public string roomName;

    public GameObject rootPlayerPanel;
    public Image characterImage;
    public List<CharacterData> characters;
    public Text className;
	public Text flavorText;
    public Text roomNameText;
    public List<GameObject> tempPanels;

    public List<PlayerPanel> playerPanels = new List<PlayerPanel>();
	private int characterIndex = 0;

    public override void NavigateTo()
    {
        base.NavigateTo();

        roomNameText.text = roomName;
        ResetPlayerPanels();
        UpdateCharacterPanel();
    }

    private void ResetPlayerPanels()
    {
        playerPanels = new List<PlayerPanel>();
        for (int i = 0; i < tempPanels.Count; i++)
        {
            tempPanels[i].SetActive(true);
        }
    }
    
    public override void NavigateFrom()
	{
		base.NavigateFrom();

		if (NetworkWrapper.discovery.isServer)
		{
			NetworkWrapper.discovery.StopBroadcast();
			NetworkWrapper.manager.StopHost();
            
            //TODO: kick players back to the first menu
		}
		else
		{
			NetworkWrapper.manager.StopClient();

            //remove player panel on the server
            Player.localAuthorityPlayer.CmdRemovePanel();
		}
	}

    public void AddPlayerPanel(PlayerPanel panel)
    {
        panel.transform.SetParent(rootPlayerPanel.transform);

        int transformIndex = playerPanels.Count;
        playerPanels.Add(panel);
        panel.SetPlayerName(transformIndex + 1);

        tempPanels[transformIndex].gameObject.SetActive(false);
        panel.transform.SetSiblingIndex(transformIndex);
    }

    public void RemovePlayerPanel(PlayerPanel panel)
    {
        int index = playerPanels.IndexOf(panel);
        playerPanels.RemoveAt(index);

    }

    public void BackButtonClicked()
	{
		TitleUIManager.Instance.Navigate_HostJoinRoomMenu();
	}

	public void ReadyButtonClicked()
	{

    }

	public void ClassCycleLeftButtonClicked()
	{
		if (characterIndex == 0)
			characterIndex = characters.Count - 1;
		else
			characterIndex--;

		UpdateCharacterPanel();
	}

	public void ClassCycleRightButtonClicked()
	{
		if (characterIndex == (characters.Count - 1))
			characterIndex = 0;
		else
			characterIndex++;

		UpdateCharacterPanel();
	}

	/// <summary>
	/// Changes the icon and description to match the character data
	/// </summary>
	private void UpdateCharacterPanel()
	{
        className.text = characters[characterIndex].name;
		flavorText.text = characters[characterIndex].flavorText;
		characterImage.sprite = characters[characterIndex].icon;
	}
}
