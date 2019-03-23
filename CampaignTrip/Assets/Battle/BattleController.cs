using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static EnemyPrefab;
using static StatusEffect;

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

    [SerializeField] private int totalAttackTime = 5;
    [SerializeField] private RectTransform attackTimerBar;
    [SerializeField] private Image[] abilityImages;
    [SerializeField] private Text[] abilityTexts;
    [SerializeField] private Text attackTimerText;
    [SerializeField] private Text attacksLeftText;
    [SerializeField] private Text blockText;

    private Coroutine attackTimerCountdown;

	[Header("Minigames")]
	public List<string> minigameSceneNames;

	private string currentMinigame;

    [Header("Spawning")]
    [HideInInspector] public List<EnemyBase> aliveEnemies;
    
    [Tooltip("Groups of enemies to spawn together.")]
    [SerializeField] private EnemyDataList enemyDataList;
    [SerializeField] private Wave[] waves;

    [Header("Misc")]
    [SerializeField] private List<StatusEffect> statusEffects;
    
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
			throw new Exception("There can only be one BattleController.");
		Instance = this;

		homeSceneName = SceneManager.GetActiveScene().name;
        NetworkWrapper.OnEnterScene(NetworkWrapper.Scene.Battle);

        PersistentPlayer.localAuthority.CmdSpawnBattlePlayer();

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
                abilityImages[i].sprite = player.Abilities[i].ButtonIcon;
                abilityTexts[i].text = player.Abilities[i].Name;
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
        RpcLoadMinigame(minigameSceneNames.FindIndex(x => x.Equals("CardSequence")));

        battlePhase = Phase.Player;

        foreach (PersistentPlayer p in PersistentPlayer.players)
        {
            p.battlePlayer.OnPlayerPhaseStart();
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

    private IEnumerator TransitionPhase()
    {
        float timeout = Time.time + 3;
        yield return new WaitWhile(() => BattlePlayerBase.PlayersUsingAbility > 0 && timeout > Time.time);

        if (Time.time > timeout)
        {
            RpcForceCancelAbility();
        }

        foreach (PersistentPlayer p in PersistentPlayer.players)
        {
            yield return p.battlePlayer.ApplySatusEffects();
        }
        
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
        battlePhase = Phase.Enemy;
        StartCoroutine(ExecuteEnemyPhase());
	}

    [Server]
    private IEnumerator ExecuteEnemyPhase()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (EnemyBase e in aliveEnemies)
        {
            if (e.IsAlive && e.HasTargets)
            {
                e.Attack();
                yield return new WaitForSeconds(0.5f);
            }
        }

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyBase e = aliveEnemies[i];
            yield return e.ApplySatusEffects();

            if (i >= aliveEnemies.Count || e != aliveEnemies[i])
            {
                //enemy died and was removed from aliveEnemies
                i--;
            }
        }

        if (aliveEnemies.Count > 0 && IsEnemyPhase)
        {
            StartPlayerPhase();
        }
    }

    #endregion

    #region Minigames

    [ClientRpc]
    private void RpcLoadMinigame(int minigameNumber)
    {
		battleCam.Cam.enabled = false;
		currentMinigame = minigameSceneNames[minigameNumber];
        SceneManager.LoadScene(currentMinigame, LoadSceneMode.Additive);
		StartCoroutine(SetActiveSceneDelayed(currentMinigame));
		battleCanvas.enabled = false;
    }

	private IEnumerator SetActiveSceneDelayed(string sceneName)
	{
		yield return 0; //makes it wait a single frame since scenes loaded additivly always load on the next frame
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)); // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.SetActiveScene.html
	}

	public void UnloadMinigame(bool succ)
    {
		if (!(currentMinigame == ""))
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(homeSceneName)); // can't find the scene were in, only the active scene
			SceneManager.UnloadScene(currentMinigame);
            battleCanvas.enabled = true;
            battleCam.Cam.enabled = true;
		}
		else
        {
			throw new Exception("There is no Minigame in scene however one it trying to be removed");
        }
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

    public StatusEffect GetStatusEffect(StatusEffectType type)
    {
        foreach (StatusEffect s in statusEffects)
        {
            if (s.Type == type)
                return s;
        }

        Debug.LogErrorFormat("Status Effect {0} not found on BattleController", type);
        return null;
    }

    protected void Win()
    {
        //TODO
    }
}
#pragma warning restore CS0618, 0649 