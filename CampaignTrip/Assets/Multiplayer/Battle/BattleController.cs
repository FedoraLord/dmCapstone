using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattleController : NetworkBehaviour
{
	public static BattleController instance;
	[Tooltip("Groups of enemies to spawn together.")]
	public GameObject[][] waves;
	protected int waveIndex = 0;
	public List<Enemy> enemies;

	protected void Start()
	{
		if (instance)
			throw new System.Exception("There can only be one BattleController.");
		instance = this;
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
		foreach(GameObject g in waves[waveIndex])
		{
			GameObject newEnemy = Instantiate(g);
			enemies.Add(newEnemy.GetComponent<Enemy>());
			NetworkServer.Spawn(newEnemy);
		}
		waveIndex++;
	}
}
