using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

#pragma warning disable CS0618, 0649
public class BattleController : NetworkBehaviour
{
	public static BattleController Instance;

    public bool IsEnemyPhase { get { return battlePhase == Phase.Enemy; } }
    public bool IsPlayerPhase { get { return battlePhase == Phase.Player; } }
    public bool IsWaitingPhase { get { return !IsEnemyPhase && !IsPlayerPhase; } }

    private bool AllPlayersReady { get { return playersReady == PersistentPlayer.players.Count; } }
    private bool AllEnemiesReady { get { return enemiesReady == waves[0].Members.Length; } }

	private string homeSceneName; // we need this because you can only find the active scene, not the scene the object is in with Scene Manager

	[Header("UI")]
	public GameObject battleCanvas;
    public List<EnemyUI> enemyUI;
    public List<HealthBarUI> playerHealthBars;

    [SerializeField] private int totalAttackTime = 5;
    [SerializeField] private RectTransform attackTimerBar;
    [SerializeField] private Text attackTimerText;

    private Coroutine attackTimerCountdown;

    [Header("Spawning")]
    [Tooltip("Groups of enemies to spawn together.")]
	public Wave[] waves;
    public Camera cam;

    [HideInInspector] public List<Enemy> aliveEnemies;
    [HideInInspector] public List<Vector3> playerSpawnPoints;
    [HideInInspector] public List<Vector3> enemySpawnPoints;

    [SerializeField] private RectTransform playerSpawnArea;
    [SerializeField] private RectTransform enemySpawnArea;

    private int enemiesReady;
    private int playersReady;
	private int waveIndex;
    private Phase battlePhase;

    [Serializable]
	public class Wave
	{
		public GameObject[] Members { get { return new GameObject[] { enemy1, enemy2, enemy4, enemy4 }; } }
               
        public GameObject enemy1;
        public GameObject enemy2;
        public GameObject enemy3;
        public GameObject enemy4;
    }

    private enum Phase { StartingBattle, Player, Enemy }

    #region Initialization

    protected void Start()
	{
        if (Instance)
			throw new Exception("There can only be one BattleController.");
		Instance = this;

		homeSceneName = SceneManager.GetActiveScene().name;

        NetworkWrapper.OnEnterScene(NetworkWrapper.Scene.Battle);

        StartBattle();
        CalculateSpawnPoints();
        PersistentPlayer.localAuthority.CmdSpawnBattlePlayer();
    }

    public HealthBarUI ClaimPlayerUI(BattlePlayer player)
    {
        foreach (HealthBarUI ui in playerHealthBars)
        {
            if (!ui.isClaimed)
            {
                ui.Claim(player.uiTransform.position, player.maxHealth, cam);
                return ui;
            }
        }
        Debug.LogError("Could not claim HealthBarUI because all HealthBarUI are already claimed.");
        return null;
    }

    [Server]
    public void OnPlayerReady()
    {
        playersReady++;
    }

    [Server]
    public void OnEnemyReady()
    {
        enemiesReady++;
    }

    private IEnumerator DelayExecution(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
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

        SpawnWave();
        yield return new WaitUntil(() => AllEnemiesReady);

        //simulate a pause where something will happen
        yield return new WaitForSeconds(1);

        StartPlayerPhase();
    }

    [Server]
    public void StartPlayerPhase()
    {
        battlePhase = Phase.Player;

        foreach (PersistentPlayer p in PersistentPlayer.players)
        {
            p.battlePlayer.OnPlayerPhaseStart();
        }

        foreach (Enemy e in aliveEnemies)
        {
            e.OnAttackTimerBegin();
        }
        RpcStartAttackTimer(totalAttackTime);
    }
    
    [Server]
    public void StartEnemyPhase()
    {
        battlePhase = Phase.Enemy;
		//StartCoroutine(ExecuteEnemyPhase()); 
        RpcLoadSwitchMaze();
    }

    [ClientRpc]
    private void RpcLoadSwitchMaze()
    {
        SceneManager.LoadScene("SwitchMaze", LoadSceneMode.Additive);
		StartCoroutine(SetActiveSceneDelayed("SwitchMaze"));
		battleCanvas.SetActive(false);
        StartCoroutine(UnloadSwitchMaze());
    }

	private IEnumerator SetActiveSceneDelayed(string sceneName)
	{
		yield return 0; //makes it wait a single frame since scenes loaded additivly always load on the next frame
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)); // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.SetActiveScene.html
	}

	private IEnumerator UnloadSwitchMaze()
    {
        yield return new WaitForSeconds(3);
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(homeSceneName)); // can't find the scene were in, only the active scene
		SceneManager.UnloadScene("SwitchMaze");
		battleCanvas.SetActive(true);
	}

    private IEnumerator ExecuteEnemyPhase()
    {
        foreach (Enemy e in aliveEnemies)
        {
            e.Attack();
        }

        foreach (PersistentPlayer p in PersistentPlayer.players)
        {
            p.battlePlayer.TakeAccumulatedDamage();
        }

        //simulate a pause where something will happen
        yield return new WaitForSeconds(1);

        StartPlayerPhase();
    }

    #endregion

    #region Spawning

    private void CalculateSpawnPoints()
    {
        //I got tired of dealing with canvas positioning so now it just gets the 
        //center of an area and calculates world coordinate offsets for each player/enemy

        Vector3 center = cam.ScreenToWorldPoint(playerSpawnArea.position);
        center.z = 0;

        playerSpawnPoints = new List<Vector3>()
        {
            center + 0.6f * Vector3.up,     //Player 1's spawn point
            center + 0.6f * Vector3.down,   //Player 2's ... etc
            center + 1.0f * Vector3.right,
            center + 1.0f * Vector3.left
        };

        center = cam.ScreenToWorldPoint(enemySpawnArea.position);
        center.z = 0;
        
        enemySpawnPoints  = new List<Vector3>()
        {
            center + 1.5f * Vector3.left,
            center + 0.5f * Vector3.left + 0.7f * Vector3.up,
            center + 0.5f * Vector3.left + 0.7f * Vector3.down,
            center + 0.5f * Vector3.right,
            center + 1.5f * Vector3.right + 0.7f * Vector3.up,
            center + 1.5f * Vector3.right + 0.7f * Vector3.down
        };
    }
    
	[Server]
	public void SpawnWave()
	{
		//Are all the waves done with?
		if (waveIndex == waves.Length)
		{
			Win();
			return;
		}

        //Spawn the next wave then
        for (int i = 0; i < waves[waveIndex].Members.Length; i++)
        {
            GameObject newEnemy = Instantiate(waves[waveIndex].Members[i], enemySpawnPoints[i], Quaternion.identity);
            aliveEnemies.Add(newEnemy.GetComponent<Enemy>());
            NetworkServer.Spawn(newEnemy);
        }

        waveIndex++;
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
        StopCoroutine(attackTimerCountdown);
    }

    #endregion

    #region Enemy

    public EnemyUI ClaimEnemyUI(Enemy enemy)
    {
        foreach (EnemyUI ui in enemyUI)
        {
            if (!ui.isClaimed)
            {
                ui.Claim(enemy.uiTransform.position, enemy.maxHealth, cam);
                return ui;
            }
        }
        Debug.LogError("Could not claim EnemyUI because all EnemyUI are already claimed.");
        return null;
    }

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
            attackTimerBar.localScale = new Vector3(timeRemaining / totalTime, 1);
            attackTimerText.text = timeRemaining.ToString(" 0.0");

            //decrement timer
            timeRemaining -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        while (timeRemaining < 0);
        
        attackTimerCountdown = null;

        if (isServer)
        {
            StartEnemyPhase();
        }
    }

    public void OnEnemyDeath(Enemy dead)
    {
        aliveEnemies.Remove(dead);
        if (aliveEnemies.Count == 0)
        {
            EndWave();
        }
    }

    #endregion

    protected void Win()
    {
        //TODO
    }
}
#pragma warning restore CS0618, 0649 