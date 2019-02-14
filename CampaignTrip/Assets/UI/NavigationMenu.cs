using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make your menu scripts inherit from this. (see HostJoinRoomMenu.cs and RoomSessionMenu.cs)
/// UIManager.cs calls NavigateTo() and NavigateFrom() when switching menus.
/// </summary>
public class NavigationMenu : MonoBehaviour
{
    /// <summary>
    /// Makes this menu visible. Override it to run some setup code when navigating to it.
    /// </summary>
    public virtual void NavigateTo()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Makes this menu invisible. Override it to run some setup code when navigating away from it.
    /// </summary>
    public virtual void NavigateFrom()
    {
        gameObject.SetActive(false);
    }
}
