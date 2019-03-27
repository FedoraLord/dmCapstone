using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class EnemyBase : BattleActorBase
{
    public bool HasTargets { get { return targets != null && targets.Length > 0; } }

    private int[] targets;
    private int remainingBlock;

    #region Initialization

    private void Start()
    {
        BattleController.Instance.OnEnemySpawned(this);
        Initialize();
    }
    
    #endregion

    #region Damage

    private void OnMouseUpAsButton()
	{
        if (IsAlive)
        {
            BattlePlayerBase localPlayer = BattlePlayerBase.LocalAuthority;
            if (localPlayer.IsValidTarget(this))
            {
                if (localPlayer.SelectedAbility != null)
                {
                    localPlayer.OnAbilityTargetChosen(this);
                }
                else
                {
                    localPlayer.CmdAttack(gameObject);
                }
            }
        }
    }
    
    public override void TakeBlockedDamage(int damage)
    {
        int initialBlock = remainingBlock;

        if (remainingBlock > 0)
        {
            remainingBlock = Mathf.Max(remainingBlock - damage, 0);
            damage -= remainingBlock;
        }

        RpcTakeDamage(damage, initialBlock - remainingBlock);
    }

    protected override void Die()
    {
        BattleController.Instance.OnEnemyDeath(this);
        StartCoroutine(DelayDeath());
    }

    private IEnumerator DelayDeath()
    {
        //TODO: play death animation
        if (isServer)
        {
            yield return new WaitForSeconds(0.5f);
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attack

    public void OnPlayerPhaseStart()
    {
        remainingBlock = blockAmount;
        
        if (HasStatusEffect(StatusEffect.Stun))
        {
            RpcUpdateTargets(new int[0]);
        }
        else
        {
            List<PersistentPlayer> visiblePlayers = PersistentPlayer.players.Where(x => !x.battlePlayer.HasStatusEffect(StatusEffect.Invisible)).ToList();
            targets = ChooseTargets(visiblePlayers);
            RpcUpdateTargets(targets);
        }
    }

    //picks targets randomly unless overridden
    protected virtual int[] ChooseTargets(List<PersistentPlayer> validTargets)
    {
        int n = Mathf.Clamp(attacksPerTurn, 0, validTargets.Count);
        List<int> choices = new List<int>();

        for (int i = 0; i < n; i++)
        {
            choices.Add(i);
        }

        List<int> result = new List<int>();
        while (n > 0)
        {
            int i = Random.Range(0, choices.Count);
            result.Add(choices[i]);
            choices.RemoveAt(i);
            n--;
        }

        return result.ToArray();
    }

    [ClientRpc]
    private void RpcUpdateTargets(int[] newTargets)
    {
        targets = newTargets;
        HealthBar.SetTargets(targets);
    }
    
    [Server]
    public void Attack()
    {
        foreach (int t in targets)
        {
            if (TryAttack())
            {
                PersistentPlayer.players[t].battlePlayer.DispatchDamage(this, basicDamage, true);
            }
        }
    }

    public void RemoveTarget(int playerNum)
    {
        List<int> targetsCopy = new List<int>(targets);
        if (targetsCopy.Remove(playerNum - 1))
        {
            targets = targetsCopy.ToArray();
            healthBarUI.SetTargets(targets);
        }
    }

    #endregion

    #region StatusEffects

    protected override void OnAddStun()
    {
        base.OnAddStun();
        HealthBar.SetTargets();

        if (isServer)
        {
            RpcUpdateTargets(new int[0]);
        }
    }

    #endregion

    public virtual void OnMinigameSuccess()
    {

    }

    public virtual void OnMinigameFailed()
    {

    }
}
#pragma warning restore CS0618, 0649