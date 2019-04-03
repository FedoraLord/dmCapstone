using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Door : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;
    public SpriteRenderer spriteRenderer;

    public List<SM_Switch> multiInputSwitches = new List<SM_Switch>();
    public List<SM_Switch> singleInputSwitches = new List<SM_Switch>();

    public BoxCollider2D closedCollider;
    public BoxCollider2D openColliderLeft;
    public BoxCollider2D openColliderRight;

    public AudioClip doorOpenAudio;
    public AudioClip doorCloseAudio;
    private AudioSource audioSource;

    private bool isOpen;
    private int multiInputDown;
    private int singleInputDown;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        for (int i = 0; i < singleInputSwitches.Count; i++)
        {
            singleInputSwitches[i].RegisterDoor(this);
        }

        for (int i = 0; i < multiInputSwitches.Count; i++)
        {
            multiInputSwitches[i].RegisterDoor(this);
        }
    }

    public void SwitchDown(SM_Switch button)
    {
        int i = singleInputSwitches.IndexOf(button);
        if (i != -1)
        {
            singleInputDown++;
        }

        i = multiInputSwitches.IndexOf(button);
        if (i != -1)
        {
            multiInputDown++;
        }

        if (singleInputDown > 0 || multiInputDown == multiInputSwitches.Count)
        {
            Open();
        }
    }

    public void SwitchUp(SM_Switch button)
    {
        int i = singleInputSwitches.IndexOf(button);
        if (i != -1)
        {
            singleInputDown--;
        }

        i = multiInputSwitches.IndexOf(button);
        if (i != -1)
        {
            multiInputDown--;
        }

        if (singleInputDown == 0 && (multiInputDown == 0 || multiInputDown < multiInputSwitches.Count))
        {
            Close();
        }
    }

    private void Open()
    {
        if (isOpen)
            return;
        
        audioSource.PlayOneShot(doorOpenAudio);
        isOpen = true;

        spriteRenderer.sprite = openSprite;

        closedCollider.enabled = false;
        openColliderLeft.enabled = true;
        openColliderRight.enabled = true;
    }

    private void Close()
    {
        if (!isOpen)
            return;

        audioSource.PlayOneShot(doorCloseAudio);
        isOpen = false;

        spriteRenderer.sprite = closedSprite;

        closedCollider.enabled = true;
        openColliderLeft.enabled = false;
        openColliderRight.enabled = false;
    }
}
