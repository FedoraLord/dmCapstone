using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HostJoinRoomMenu : NavigationMenu
{
    [SerializeField] private GameObject roomButtonTemplate;

    private List<GameObject> roomButtons = new List<GameObject>();

    public override void NavigateTo()
    {
        base.NavigateTo();
        NetworkWrapper.discovery.ListenForLANServers();
    }

    public override void NavigateFrom()
    {
        base.NavigateFrom();
        NetworkWrapper.discovery.StopListening();
    }

    public GameObject AddRoom(string text)
    {
        GameObject btn = Instantiate(roomButtonTemplate, roomButtonTemplate.transform.parent);

        Text txt = btn.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.text = text;
        }

        btn.SetActive(true);
        roomButtons.Add(btn);

        return btn;
    }

    public void RemoveRoom(GameObject btn)
    {
        roomButtons.Remove(btn);
        Destroy(btn);
    }

    public void RoomSelected(GameObject buttonClicked)
    {
        Text t = buttonClicked.GetComponentInChildren<Text>();
        if (t != null)
        {
            string ipAddress = NetworkWrapper.discovery.GetAddressOfRoom(t.text);
            if (ipAddress != null)
            {
                NetworkWrapper.discovery.StopListening();

                NetworkWrapper.manager.networkAddress = ipAddress;
                NetworkWrapper.manager.StartClient();

                NavigateToRoomSessionMenu(t.text);
            }
            else
            {
                Debug.LogErrorFormat("No room of name {0}", t.text);
            }
        }
        else
        {
            Debug.LogError("Cannot get name of room selected.");
        }
    }

    public void CreateRoomButtonClicked(InputField roomName)
    {
        if (roomName.text.Length == 0)
            return;

        NetworkWrapper.discovery.broadcastData = roomName.text;
        NetworkWrapper.discovery.BroadcastAsServer();

        NetworkWrapper.manager.StartHost();

        NavigateToRoomSessionMenu(roomName.text);
    }

    private void NavigateToRoomSessionMenu(string roomName)
    {
        TitleUIManager.Instance.roomSessionMenu.roomName = roomName;
        TitleUIManager.Instance.Navigate_RoomSessionMenu();
    }
}
