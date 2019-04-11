using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public class CheatMenu : NetworkBehaviour
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        isEditor = true;
#endif

        ToggleCheats(minigameCheats, false);
        ToggleCheats(battleCheats, isEditor);
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
        CmdKillEnemies();
    }

    [Command]
    private void CmdKillEnemies()
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
