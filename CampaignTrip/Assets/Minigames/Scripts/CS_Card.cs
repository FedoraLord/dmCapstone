using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class CS_Card : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite cardBack;

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
            CS_Manager manager = (MinigameManager.Instance as CS_Manager);
            if (manager.CanSelect)
            {
                isSelectable = false;
                spriteRenderer.sprite = cardBack;
                manager.CardSelected(this);
            }
        }
    }
}
