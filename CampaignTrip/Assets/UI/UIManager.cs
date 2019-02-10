using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parent class to handle menu navigations easier
/// </summary>
public class UIManager : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] private NavigationMenu startingMenu;
#pragma warning restore CS0649

    private NavigationMenu currentMenu;

    protected virtual void Start()
    {
        if (startingMenu != null)
        {
            NavigationMenu[] menus = GetComponentsInChildren<NavigationMenu>();
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i] != startingMenu)
                {
                    menus[i].gameObject.SetActive(false);
                }
            }
            Navigate(startingMenu);
        }
    }

    protected void Navigate(NavigationMenu menu)
    {
        if (currentMenu != null)
        {
            currentMenu.NavigateFrom();
        }
        currentMenu = menu;
        currentMenu.NavigateTo();
    }
}
