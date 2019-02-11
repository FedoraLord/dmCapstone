using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerPanel : NetworkBehaviour
{
    public Text playerName;
    public Image selectedClass;
    public Image readyState;

    public void SetPlayerName(int pnum)
    {
        playerName.text = "P" + pnum;
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    private void Start()
    {
        TitleUIManager.Instance.roomSessionMenu.AddPlayerPanel(this);
    }
}
