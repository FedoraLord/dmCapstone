using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    public static TitleUIManager Instance;

    public HostJoinRoomMenu hostJoinRoomMenu;
    public RoomSessionMenu roomSessionMenu;
    
    [SerializeField] private NavigationMenu startingMenu;

    private NavigationMenu currentMenu;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple TitleUIManager objects exist.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (startingMenu != null)
        {
            Navigate(startingMenu);
        }
    }

    private void Navigate(NavigationMenu menu)
    {
        if (currentMenu != null)
        {
            currentMenu.NavigateFrom();
        }
        currentMenu = menu;
        currentMenu.NavigateTo();
    }

    public void Navigate_HostJoinRoomMenu()
    {
        Navigate(hostJoinRoomMenu);
    }

    public void Navigate_RoomSessionMenu()
    {
        Navigate(roomSessionMenu);
    }
}
