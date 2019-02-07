using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationMenu : MonoBehaviour
{
    public virtual void NavigateTo()
    {
        gameObject.SetActive(true);
    }

    public virtual void NavigateFrom()
    {
        gameObject.SetActive(false);
    }
}
