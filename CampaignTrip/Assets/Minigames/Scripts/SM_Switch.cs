using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Switch : MonoBehaviour
{
    public Sprite offSprite;
    public Sprite onSprite;

    private SpriteRenderer sr;
    private List<SM_Door> doors = new List<SM_Door>();

    private int numPlayersOnSwitch = 0;
    
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void RegisterDoor(SM_Door door)
    {
        doors.Add(door);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            sr.sprite = onSprite;
            numPlayersOnSwitch++;

            if (numPlayersOnSwitch == 1)
            {
                for (int i = 0; i < doors.Count; i++)
                {
                    doors[i].SwitchDown(this);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sr.sprite = offSprite;
            numPlayersOnSwitch--;

            if (numPlayersOnSwitch == 0)
            {
                for (int i = 0; i < doors.Count; i++)
                {
                    doors[i].SwitchUp(this);
                }
            }
        }
    }
}
