using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class CS_Card : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [HideInInspector] public int index = -1;
    [HideInInspector] public bool isSelectable;

    public void InitializeSelectable(int i)
    {
        index = i;
        isSelectable = true;
    }

    private void OnMouseUpAsButton()
    {
        if (isSelectable)
        {
            (MinigameManager.Instance as CS_Manager).CardSelected(this);
        }
    }
}
