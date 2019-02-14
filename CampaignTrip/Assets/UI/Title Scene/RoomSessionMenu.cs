using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoomSessionMenu : NavigationMenu
{
    [HideInInspector] public string roomName;

    public bool isReady;
    public GameObject rootPlayerPanel;
    public Image characterImage;
    public List<CharacterData> characters;
    public Text className;
	public Text flavorText;
    public Text roomNameText;
	private int characterIndex = 0;

    public override void NavigateTo()
    {
        base.NavigateTo();

        roomNameText.text = roomName;
        isReady = false;
    }
    
    public override void NavigateFrom()
	{
		base.NavigateFrom();
	}

    public void BackButtonClicked()
	{
        if (NetworkWrapper.discovery.isServer)
        {
            NetworkWrapper.discovery.StopBroadcast();
            NetworkWrapper.manager.StopHost();

            //TODO: kick players back to the first menu

            TitleUIManager.Navigate_HostJoinRoomMenu();
        }
        else
        {
            StartCoroutine(ClientLeave());
        }
	}

    private IEnumerator ClientLeave()
    {
        Player.localAuthority.CmdDisconnect();
        
        //Need to wait a little before disconnecting so we can call the server Command method.
        yield return new WaitForSeconds(0.2f);

        NetworkWrapper.manager.StopClient();
        TitleUIManager.Navigate_HostJoinRoomMenu();
    }

    public void ReadyButtonClicked()
	{
        isReady = !isReady;
        Player.localAuthority.CmdUpdatePanel(characterIndex, isReady);
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
	public void UpdateCharacterPanel()
	{
        className.text = characters[characterIndex].name;
		flavorText.text = characters[characterIndex].flavorText;
		characterImage.sprite = characters[characterIndex].icon;

        Player.localAuthority.CmdUpdatePanel(characterIndex, isReady);
	}
}
