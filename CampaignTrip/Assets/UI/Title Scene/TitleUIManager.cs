using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles menu navigation in our Title Scene
/// </summary>
public class TitleUIManager : UIManager
{
    public static TitleUIManager Instance;

    public HostJoinRoomMenu hostJoinRoomMenu;
    public RoomSessionMenu roomSessionMenu;
    
    protected override void Start()
    {
        base.Start();

        if (Instance != null)
        {
            Debug.LogError("Multiple TitleUIManager objects exist.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
