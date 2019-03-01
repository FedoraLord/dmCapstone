using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattlePlayer : NetworkBehaviour
{
    public static BattlePlayer LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }

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
        persistentPlayer.battlePlayer = this;
        transform.position = BattleController.Instance.playerSpawnPoints[i];
    }

    [Command]
    public void CmdAttack(GameObject target)
    {
        Enemy enemy = target.GetComponent<Enemy>();
        //play attack animation on clients
        enemy.TakeDamage(20);
    }

    private void UpdateHealthUI(int hp)
    {

    }
}
