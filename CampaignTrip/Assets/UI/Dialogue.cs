using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public Text messageText;

    public void DisplayMessage(string message, float seconds, Action callback = null)
    {
        messageText.text = message;
        gameObject.SetActive(true);
        StartCoroutine(ShowForSeconds(seconds, callback));
    }

    private IEnumerator ShowForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
        callback?.Invoke();
    }
}
