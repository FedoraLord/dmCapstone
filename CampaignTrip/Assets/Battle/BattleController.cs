﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static BattleActorBase;
using static EnemyPrefab;

#pragma warning disable CS0618, 0649
public class BattleController : NetworkBehaviour
{
	public static BattleController Instance;

    public bool IsEnemyPhase { get { return battlePhase == Phase.Enemy; } }
    public bool IsPlayerPhase { get { return battlePhase == Phase.Player; } }
    public bool IsWaitingPhase { get { return !IsEnemyPhase && !IsPlayerPhase; } }
    public Camera MainCamera { get { return battleCam.Cam; } }
    
    private bool AllPlayersReady { get { return playersReady == PersistentPlayer.players.Count; } }
    private bool AllEnemiesReady { get { return enemiesReady == waves[waveIndex].members.Count; } }

	private string homeSceneName; // we need this because you can only find the active scene, not the scene the object is in with Scene Manager

	[Header("UI")]
    public BattleCamera battleCam;
	public Canvas battleCanvas;

    [SerializeField] private float totalAttackTime = 5;
    [SerializeField] private RectTransform attackTimerBar;
    [SerializeField] private AbilityButton[] abilityButtons;
    [SerializeField] private Text attackTimerText;
    [SerializeField] private Text attacksLeftText;
    [SerializeField] private Text blockText;

    private Coroutine attackTimerCountdown;

    [Header("Minigames")]
    public List<string> minigameSceneNames;
    public bool TEST_GoToMinigame;
    public string TEST_ForceMinigameSceneName;

    [Header("Spawning")]
    [HideInInspector] public List<EnemyBase> aliveEnemies;
    
    [SerializeField] private EnemyDataList enemyDataList;
    [Tooltip("Groups of enemies to spawn together.")]
    [SerializeField] private Wave[] waves;

    //[Header("Misc")]
    private Dictionary<StatusEffect, int> dotStatusEffects = new Dictionary<StatusEffect, int>();
    private int enemiesReady;
    private int playersReady;
	private int waveIndex = -1;
    private Phase battlePhase;

    [Serializable]
	public class Wave
	{
        public List<EnemyType> members;

        public List<GameObject> GetEnemyPrefabs(EnemyDataList data)
        {
            List<GameObject> prefabs = new List<GameObject>();
            foreach (EnemyType type in members)
            {
                prefabs.Add(data.GetPrefab(type));
            }
            return prefabs;
        }
    }

    private enum Phase { StartingBattle, Player, Transition, Enemy }

    #region Initialization

    private void OnValidate()
    {
        foreach (Wave w in waves)
        {
            if (w.members.Count > 6)
            {
                w.members.RemoveRange(6, w.members.Count - 6);
            }
        }
    }
    
    protected void Start()
	{
        if (Instance)
        {
            //coming back after a minigame:
            Destroy(gameObject);
            Destroy(battleCam.gameObject);
            return;
        }

		Instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(battleCam.gameObject);

		homeSceneName = SceneManager.GetActiveScene().name;
        NetworkWrapper.OnEnterScene(NetworkWrapper.Scene.Battle);

        PersistentPlayer.localAuthority.CmdSpawnBattlePlayer();

        dotStatusEffects.Add(StatusEffect.Bleed, 5);
        dotStatusEffects.Add(StatusEffect.Burn, 0); //damage = duration
        dotStatusEffects.Add(StatusEffect.Poison, 5);

        if (isServer)
        {
            StartBattle();
        }
    }

    public void OnBattlePlayerSpawned(BattlePlayerBase player)
    {
        if (player == BattlePlayerBase.LocalAuthority)
        {
            for (int i = 0; i < player.Abilities.Count; i++)
            {
                player.Abilities[i].SetButton(abilityButtons[i]);
            }
        }

        if (isServer)
        {
            playersReady++;
        }
    }

    #endregion

    #region Flow

    [Server]
    public void StartBattle()
    {
        battlePhase = Phase.StartingBattle;
        //playersReady = 0;
        enemiesReady = 0;
        StartCoroutine(ExecuteStartingBattlePhase());
    }

    private IEnumerator ExecuteStartingBattlePhase()
    {
        yield return new WaitUntil(() => AllPlayersReady);

        //simulate a pause where something will happen
        yield return new WaitForSeconds(1);

        if (SpawnWave())
        {
            yield return new WaitUntil(() => AllEnemiesReady);

            //simulate a pause where something will happen
            yield return new WaitForSeconds(1);

            StartPlayerPhase();
        }
    }
    
    [Server]
    private void StartPlayerPhase()
    {
        battlePhase = Phase.Player;

        foreach (BattlePlayerBase p in BattlePlayerBase.players)
        {
            p.OnPlayerPhaseStart();
        }

        foreach (EnemyBase e in aliveEnemies)
        {
            e.OnPlayerPhaseStart();
        }
        RpcStartAttackTimer(totalAttackTime);
    }

    [Server]
    private void StartTransitionPhase()
    {
        battlePhase = Phase.Transition;
        StartCoroutine(TransitionPhase());
    }

    [Server]
    private IEnumerator TransitionPhase()
    {
        float timeout = Time.time + 3;
        yield return new WaitWhile(() => BattlePlayerBase.PlayersUsingAbility > 0 && timeout > Time.time);

        if (Time.time > timeout)
        {
            RpcForceCancelAbility();
        }

        yield return ApplyDOTs(BattlePlayerBase.players);

        StartEnemyPhase();
    }

    [ClientRpc]
    private void RpcForceCancelAbility()
    {
        if (BattlePlayerBase.LocalAuthority.SelectedAbility != null)
        {
            BattlePlayerBase.LocalAuthority.EndAbility();
        }
    }

    [Server]
    private void StartEnemyPhase()
    {
        if (TEST_GoToMinigame)
        {
            TEST_GoToMinigame = false;

            if (string.IsNullOrEmpty(TEST_ForceMinigameSceneName))
            {
                int randomIndex = UnityEngine.Random.Range(0, minigameSceneNames.Count);
                LoadMinigame(minigameSceneNames[randomIndex]);
            }
            else
            {
                LoadMinigame(TEST_ForceMinigameSceneName);
            }

            return;
        }

        battlePhase = Phase.Enemy;
        StartCoroutine(ExecuteEnemyPhase());
	}

    public class AttackInfo
    {
        public bool containsMiss;
        public List<BattleActorBase> attackers = new List<BattleActorBase>();
    }

    [Server]
    private IEnumerator ExecuteEnemyPhase()
    {
        yield return new WaitForSeconds(0.5f);
        
        //get attack damage directed at each player
        AttackInfo[] attacks = new AttackInfo[BattlePlayerBase.players.Count];
        attacks.Initialize(() => new AttackInfo());

        foreach (EnemyBase e in aliveEnemies)
        {
            if (e.IsAlive && e.HasTargets)
                e.AttackPlayers(attacks);
        }

        //apply attack damage on the players
        for (int i = 0; i < BattlePlayerBase.players.Count; i++)
        {
            BattlePlayerBase bp = BattlePlayerBase.players[i];
            if (attacks[i].containsMiss)
            {
                bp.RpcMiss();
                yield return new WaitForSeconds(0.5f);
            }

            if (attacks[i].attackers.Count > 0)
            {
                bp.DispatchBlockableDamage(attacks[i].attackers);
                yield return new WaitForSeconds(0.5f);
            }
        }

        //apply damage over time status effects on enemies
        yield return ApplyDOTs(aliveEnemies);

        DecrementStats(aliveEnemies);
        DecrementStats(BattlePlayerBase.players);

        yield return new WaitForSeconds(0.1f);
        
        if (aliveEnemies.Count > 0 && IsEnemyPhase)
        {
            StartPlayerPhase();
        }
    }

    [Server]
    private IEnumerator ApplyDOTs<T>(List<T> actors) where T : BattleActorBase
    {
        List<StatusEffect> dots = new List<StatusEffect>(dotStatusEffects.Keys);
        for (int i = 0; i < dots.Count; i++)
        {
            bool tookStatDamage = false;
            foreach (BattleActorBase actor in actors)
            {
                if (actor.ApplyDOT(dots[i]))
                    tookStatDamage = true;
            }

            if (tookStatDamage)
                yield return new WaitForSeconds(0.5f);
        }
    }

    [Server]
    private void DecrementStats<T>(List<T> actors) where T : BattleActorBase
    {
        foreach (BattleActorBase actor in actors)
        {
            actor.RpcDecrementDurations();
        }
    }

        #endregion

        #region Minigames

    [Server]
    private void LoadMinigame(string sceneName)
    {
        RpcLoadScene(false);
        PersistentPlayer.localAuthority.minigameReady = 0;
        NetworkWrapper.manager.ServerChangeScene(sceneName);
    }

    [ClientRpc]
    private void RpcLoadScene(bool isBattleScene)
    {
        MainCamera.enabled = isBattleScene;
        battleCanvas.enabled = isBattleScene;
    }

    [Server]
    public void UnloadMinigame(bool succ)
    {
        StartPlayerPhase();
        RpcLoadScene(true);
        NetworkWrapper.manager.ServerChangeScene("Battle");
    }

    #endregion

    #region Spawning

    public void OnEnemySpawned(EnemyBase enemy)
    {
        if (isServer)
        {
            enemiesReady++;
        }

        int i = aliveEnemies.Count;
        enemy.transform.position = battleCam.EnemySpawnPoints[i].position;
        aliveEnemies.Add(enemy);
    }
    
	[Server]
	public bool SpawnWave()
	{
        waveIndex++;

        //Are all the waves done with?
        if (waveIndex == waves.Length)
		{
			Win();
			return false;
		}

        //Spawn the next wave then
        List<GameObject> enemyPrefabs = waves[waveIndex].GetEnemyPrefabs(enemyDataList);

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (enemyPrefabs[i] == null)
                continue;

            GameObject newEnemy = Instantiate(enemyPrefabs[i]);
            NetworkServer.Spawn(newEnemy);
        }

        return true;
    }

    [Server]
    public void EndWave()
    {
        RpcStopAttackTimer();
        StartBattle();
    }

    [ClientRpc]
    private void RpcStopAttackTimer()
    {
        if (attackTimerCountdown != null)
        {
            StopCoroutine(attackTimerCountdown);
        }
    }

    #endregion

    #region Enemy
    
    [ClientRpc]
    public void RpcStartAttackTimer(float time)
    {
        if (attackTimerCountdown != null)
        {
            StopCoroutine(attackTimerCountdown);
        }
        attackTimerCountdown = StartCoroutine(AttackTimerCountdown(time));
    }

    private IEnumerator AttackTimerCountdown(float totalTime)
    {
        float timeRemaining = totalTime;
        do
        {
            //update UI
            attackTimerBar.localScale = new Vector3(timeRemaining / totalTime, 1, 1);
            attackTimerText.text = timeRemaining.ToString(" 0.0");

            //decrement timer
            timeRemaining -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        while (timeRemaining > 0);
        
        attackTimerCountdown = null;

        if (isServer)
        {
            StartTransitionPhase();
        }
    }

    public void OnEnemyDeath(EnemyBase dead)
    {
        aliveEnemies.Remove(dead);
        if (isServer && aliveEnemies.Count == 0)
        {
            EndWave();
        }
    }

    #endregion

    #region UI
    
    public void UpdateAttackBlockUI(int attacks, int block)
    {
        attacksLeftText.text = attacks.ToString();
        blockText.text = string.Format("{0}%", block);
    }

    public void OnAbilityButtonClicked(int i)
    {
        BattlePlayerBase.LocalAuthority.AbilitySelected(i);
    }

    #endregion

    public int GetDOT(StatusEffect effect)
    {
        if (dotStatusEffects.ContainsKey(effect))
            return dotStatusEffects[effect];
        return -1;
    }

    protected void Win()
    {
        //TODO
    }
}
#pragma warning restore CS0618, 0649 