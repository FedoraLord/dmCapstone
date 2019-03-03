using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleController : NetworkBehaviour
{
	public static BattleController Instance;

    [Header("UI")]
    public List<EnemyUI> enemyUI;

    [SerializeField] private RectTransform attackTimerBar;
    [SerializeField] private Text attackTimerText;

    private Coroutine attackTimerCountdown;

    [Header("Spawning")]
    [Tooltip("Groups of enemies to spawn together.")]
	public Wave[] waves;

    [HideInInspector] public List<Vector3> playerSpawnPoints;
    [HideInInspector] public List<Vector3> enemySpawnPoints;
    [HideInInspector] public List<Enemy> enemies;

    [SerializeField] private RectTransform playerSpawnArea;
    [SerializeField] private RectTransform enemySpawnArea;
    [SerializeField] public Camera cam;

	private int waveIndex = 0;

    [System.Serializable]
	public class Wave
	{
		public GameObject[] Members { get { return new GameObject[] { enemy1, enemy2, enemy4, enemy4 }; } }
               
        public GameObject enemy1;
        public GameObject enemy2;
        public GameObject enemy3;
        public GameObject enemy4;
    }

    protected void Start()
	{
        if (Instance)
			throw new System.Exception("There can only be one BattleController.");
		Instance = this;
        
        NetworkWrapper.OnEnterScene(NetworkWrapper.Scene.Battle);

        CalculateSpawnPoints();
        //TestSpawnPoints();
        PersistentPlayer.localAuthority.CmdSpawnBattlePlayer();
    }

    public EnemyUI ClaimEnemyUI(Enemy enemy)
    {
        foreach (EnemyUI ui in enemyUI)
        {
            if (!ui.claimed)
            {
                ui.Claim(enemy);
                return ui;
            }
        }
        return null;
    }

    #region Spawning

    public List<GameObject> SPAWN_TEST_PLAYERS;
    public List<GameObject> SPAWN_TEST_ENEMIES;

    private void TestSpawnPoints()
    {
        for (int i = 0; i < SPAWN_TEST_PLAYERS.Count; i++)
        {
            SPAWN_TEST_PLAYERS[i].transform.position = playerSpawnPoints[i];
        }

        for (int i = 0; i < SPAWN_TEST_ENEMIES.Count; i++)
        {
            SPAWN_TEST_ENEMIES[i].transform.position = enemySpawnPoints[i];
        }
    }
    
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
    
	[Command]
	public void CmdSpawnNewWave()
	{
		//clear old wave(this will be destroyed on the clients too automatically)
		foreach (Enemy e in enemies)
			Destroy(e.gameObject);
		enemies.Clear();

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
            enemies.Add(newEnemy.GetComponent<Enemy>());
            NetworkServer.Spawn(newEnemy);
        }

        RpcStartAttackTimer(10);

        waveIndex++;
	}

    [Command]
    public void CmdTryEndWave()
    {
        foreach (Enemy e in enemies)
            if (e.isAlive)
                return;
        CmdSpawnNewWave();
    }

    #endregion

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
        while (true)
        {
            //update UI
            attackTimerBar.localScale = new Vector3(timeRemaining / totalTime, 1);
            attackTimerText.text = timeRemaining.ToString(" 0.0");
            
            //decrement timer
            timeRemaining -= Time.deltaTime;

            if (timeRemaining < 0)
                break;

            yield return new WaitForEndOfFrame();
        }
        attackTimerCountdown = null;
    }

    protected void Win()
    {
        //TODO
    }
}
