using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static BattlePlayerBase;
using static EnemyBase;

#pragma warning disable 0618, 0649
public class BuffStatTracker : NetworkBehaviour
{
    public static BuffStatTracker Instance { get; private set; }

    [SerializeField] private EnemyDataList enemies;

    private Dictionary<CharacterType, BattleStats> playerStats;
    private Dictionary<EnemyType, BattleStats> enemyStats;

    //testing:
    //public List<EnemyType> types;
    //public List<BattleStats> stats;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        enemyStats = new Dictionary<EnemyType, BattleStats>();
        playerStats = new Dictionary<CharacterType, BattleStats>();

        foreach (EnemyPrefab enemy in enemies.EnemyPrefabList)
        {
            EnemyBase script = enemy.Prefab.GetComponent<EnemyBase>();
            if (script == null)
            {
                Debug.LogError("BuffStatTracker.Start() : Enemy prefab does not have an enemy script on it.");
                continue;
            }

            script.BattleStats.Immunities.BuffPool = StatusEffect.Debuffs.ToArray();
            script.BattleStats.AppliedEffects.BuffPool = StatusEffect.All.ToArray();
            enemyStats.Add(script.enemyType, script.BattleStats);

            //types.Add(script.enemyType);
            //stats.Add(script.BattleStats);
        }
    }

    [Server]
    public void ApplyRandomEnemyBuffs()
    {
        EnemyType[] types = (EnemyType[])Enum.GetValues(typeof(EnemyType));
        foreach (EnemyType t in types)
        {
            if (enemyStats.ContainsKey(t))
            {
                BattleStats stats = enemyStats[t].ApplyRandomBuff();
                RpcUpdateEnemyStats(t, stats);
            }
        }
    }

    public BattleStats GetPlayerStats(CharacterType type, BattleStats currentStats)
    {
        if (playerStats.ContainsKey(type))
            return playerStats[type];

        playerStats.Add(type, currentStats);
        return currentStats;
    }

    public BattleStats GetEnemyStats(EnemyType type)
    {
        return enemyStats[type];
    }

    [ClientRpc]
    public void RpcUpdatePlayerStats(CharacterType type, BattleStats stats)
    {
        playerStats[type] = stats; 
    }

    [ClientRpc]
    public void RpcUpdateEnemyStats(EnemyType type, BattleStats stats)
    {
        enemyStats[type] = stats; 
    }
}
