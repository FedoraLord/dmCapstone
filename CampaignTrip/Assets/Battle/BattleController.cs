using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattleController : NetworkBehaviour
{
	public static BattleController Instance;
	[Tooltip("Groups of enemies to spawn together.")]
	public Wave[] waves;
	public List<RectTransform> spawnPoints;
	public RectTransform[] enemySpawnPoints;
	[HideInInspector]
	public List<Enemy> enemies;
	//used for spawning stuff in the right place
	private Camera cam;

	protected int waveIndex = 0;

    [System.Serializable]
	public struct Wave
	{
		public GameObject[] members;
	}

	protected void Start()
	{
		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		// spawn the first wave
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
		if (waveIndex == waves.Length)
		{
			Win();
			return;
		}

		//Spawn the next wave then
		for (int i = 0; i < waves[waveIndex].members.Length; i++)
		{
			Vector3 pos = cam.ScreenToWorldPoint(enemySpawnPoints[i].position);
			pos.z = 0; //otherwise its on the same z as the camera and we cant see it
			GameObject newEnemy = Instantiate(waves[waveIndex].members[i], pos, Quaternion.identity);
			enemies.Add(newEnemy.GetComponent<Enemy>());
			NetworkServer.Spawn(newEnemy);
		}
		waveIndex++;
	}
}
