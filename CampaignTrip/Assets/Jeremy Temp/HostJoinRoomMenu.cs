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
        CustomNetworkDiscovery.Instance.ListenForLANServers();
    }

    public override void NavigateFrom()
    {
        base.NavigateFrom();
        CustomNetworkDiscovery.Instance.StopListening();
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

    public void CreateRoomButtonClicked(InputField roomName)
    {
        if (roomName.text.Length == 0)
            return;

        CustomNetworkDiscovery discovery = CustomNetworkDiscovery.Instance;
        discovery.broadcastData = roomName.text;
        discovery.BroadcastAsServer();
    }
}
