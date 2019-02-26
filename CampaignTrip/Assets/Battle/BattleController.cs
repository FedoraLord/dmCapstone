using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattleController : NetworkBehaviour
{
	public static BattleController Instance;

    [Tooltip("Groups of enemies to spawn together.")]
	public List<Enemy> enemies;
    public List<RectTransform> spawnPoints;
	public Wave[] waves;

    protected int waveIndex = 0;

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
		CmdSpawnNewWave();
		if (Instance)
			throw new System.Exception("There can only be one BattleController.");
		Instance = this;
        PersistentPlayer.localAuthority.CmdSpawnCharacter();
    }

	protected void Win()
	{
		//TODO
	}

	[Command]
	public void CmdTryEndWave()
	{
		foreach (Enemy e in enemies)
			if (e.isAlive)
				return;
		CmdSpawnNewWave();
	}

	[Command]
	public void CmdSpawnNewWave()
	{
		//clear old wave(this will be destroyed on the clients too automatically)
		foreach (Enemy e in enemies)
			Destroy(e.gameObject);
		enemies.Clear();

		//Are all the waves done with?
		if(waveIndex == waves.Length)
		{
			Win();
			return;
		}

		//Spawn the next wave then
		foreach(GameObject g in waves[waveIndex].Members)
		{
			GameObject newEnemy = Instantiate(g);
			enemies.Add(newEnemy.GetComponent<Enemy>());
			NetworkServer.Spawn(newEnemy);
		}
		waveIndex++;
	}
}
