using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSessionMenu : NavigationMenu
{
    [HideInInspector] public string roomName;
	public Image characterImage;
	public Text flavorText;
	public List<CharacterData> characters;

	private int characterIndex = 0;

	public override void NavigateFrom()
	{
		base.NavigateFrom();

		if (NetworkWrapper.discovery.isServer)
		{
			NetworkWrapper.discovery.StopBroadcast();
			NetworkWrapper.manager.StopHost();
		}
		else
		{
			NetworkWrapper.manager.StopClient();
		}
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
		flavorText.text = characters[characterIndex].flavorText;
		characterImage.sprite = characters[characterIndex].icon;
	}
}
