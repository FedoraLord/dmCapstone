using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public Text messageText;
    public Image img;

    private void Start()
    {
        Toggle(false);
    }

    private void Toggle(bool visible)
    {
        img.enabled = visible;
        messageText.enabled = visible;
    }
    
    public void DisplayMessage(string message, float seconds, Action callback = null)
    {
        messageText.text = message;
        Toggle(true);
        StartCoroutine(ShowForSeconds(seconds, callback));
    }

    private IEnumerator ShowForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        Toggle(false);
        callback?.Invoke();
    }
}
