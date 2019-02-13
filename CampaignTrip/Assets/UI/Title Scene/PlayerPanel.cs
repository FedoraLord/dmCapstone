using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    public Text playerName;
    public Image selectedClass;
    public Image readyState;
    public Player associatedPlayer;

    private void Start()
    {
        StartCoroutine(SetUp());
    }

    private IEnumerator SetUp()
    {
        yield return new WaitUntil(() => associatedPlayer.playerNum > 0);

        GameObject root = TitleUIManager.RoomSessionMenu.rootPlayerPanel;
        transform.SetParent(root.transform);
        SetPlayerName(associatedPlayer.playerNum);

        Vector2 size = root.GetComponent<RectTransform>().rect.size;
        size.y /= 4;
        GetComponent<RectTransform>().sizeDelta = size;

        TitleUIManager.RoomSessionMenu.UpdateCharacterPanel();
    }

    public void SetPlayerName(int pnum)
    {
        playerName.text = "P" + pnum;
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    public void UpdateUI(int characterIndex, bool isReady)
    {
        CharacterData data = TitleUIManager.RoomSessionMenu.characters[characterIndex];
        selectedClass.sprite = data.icon;
        readyState.enabled = isReady;
    }
}
