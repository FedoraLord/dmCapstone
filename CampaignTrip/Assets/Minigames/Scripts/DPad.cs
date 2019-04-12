using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public static DPad Instance;

    private SM_Player localPlayer;
    public Camera mainCamera;


    public int deadzone;

    void Start()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    public void Setup(SM_Player player)
    {
        localPlayer = player;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData ped)
    {
        Vector2 dPadPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 direction = new Vector2();
        direction.x = ped.position.x - dPadPos.x;
        direction.y = ped.position.y - dPadPos.y;
        if (direction.magnitude > deadzone)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                direction.y = 0;
            }
            else
            {
                direction.x = 0;
            }

            localPlayer.velocity = new Vector3(direction.x, direction.y, 0);
        }
    }

    public void OnPointerUp(PointerEventData ped)
    {
        localPlayer.velocity = new Vector2(0, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnPointerDown(eventData);
    }
}
