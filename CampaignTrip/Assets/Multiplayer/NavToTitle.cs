using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavToTitle : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        if (NetworkWrapper.Instance == null)
        {
            button.gameObject.SetActive(true);
        }
    }

    public void TitleButtonClicked()
    {
        SceneManager.LoadScene(0);
    }
}
