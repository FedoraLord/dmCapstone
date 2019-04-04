using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatMenu : MonoBehaviour
{
    public static CheatMenu Instance;

    public GameObject[] minigameCheats;

    private bool isEditor;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            ToggleMinigameCheats(false);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        isEditor = true;
#endif
    }

    public void ToggleMinigameCheats(bool on)
    {
        if (!isEditor && on)
            return;

        foreach (GameObject obj in minigameCheats)
        {
            obj.SetActive(on);
        }
    }

    public void WinMinigame()
    {
        BattleController.Instance.UnloadMinigame(true);
    }

    public void LoseMinigame()
    {
        BattleController.Instance.UnloadMinigame(false);
    }
}
