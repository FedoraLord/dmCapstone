using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattlePlayer : NetworkBehaviour
{
    [SyncVar(hook = "UpdateHealthUI")]
    public int health;

    [SyncVar]
    public int playerNum;

    public PersistentPlayer persistentPlayer;

    public override void OnStartClient()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => BattleController.Instance != null);

        int i = playerNum - 1;
        persistentPlayer = PersistentPlayer.players[i];
        transform.position = BattleController.Instance.playerSpawnPoints[i];
    }

    private void UpdateHealthUI(int hp)
    {

    }
}
