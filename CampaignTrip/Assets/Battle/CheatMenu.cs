using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatMenu : MonoBehaviour
{
    public static CheatMenu Instance;

    public GameObject[] battleCheats;
    public GameObject[] minigameCheats;

    private bool isEditor;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            ToggleCheats(true);
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

    public void ToggleCheats(bool isBattleScene)
    {
        ToggleCheats(battleCheats, isBattleScene);
        ToggleCheats(minigameCheats, !isBattleScene);
    }

    private void ToggleCheats(GameObject[] cheats, bool on)
    {
        if (!isEditor && on)
            return;

        foreach (GameObject obj in cheats)
        {
            obj.SetActive(on);
        }
    }

    public void KillEnemies()
    {
        foreach (EnemyBase enemy in BattleController.Instance.aliveEnemies)
        {
            enemy.TakeBlockedDamage(9999);
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
