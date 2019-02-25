using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class CharacterCreator : NetworkBehaviour
{
	/// <summary>
	/// The Singleton instance.
	/// </summary>
	public static CharacterCreator instance;
	/// <summary>
	/// Connection ID and their associated characters.
	/// </summary>
	public Dictionary<int, CharacterData> chosenCharacters;
	[Tooltip("We will use the CharacterData.battlePrefab for these scenes.")]
	public string[] battleScenes;

	protected void Start()
	{
		chosenCharacters = new Dictionary<int, CharacterData>();
		if (instance)
			throw new System.Exception("There can only be one CharacterCreator.");
		instance = this;
	}

	[Command]
	public void CmdSpawnCharacter(int playersConnectionId)
	{
		string currentSceneName = SceneManager.GetActiveScene().name;

		//TODO: port lobby PlayerPanel and minigame characters to this spawner
		if (currentSceneName == "Title" || currentSceneName == "SwitchMaze")
			return;

		//figure out what kinda scene this is

		//add more scenetypes here(that dont require a specific class)

		//scenes that need a character type chosen
		if (chosenCharacters.ContainsKey(playersConnectionId))
			throw new System.Exception("Connection " + playersConnectionId + " does not have a character type!");

		if (battleScenes.Contains(currentSceneName))
			NetworkServer.Spawn(Instantiate(chosenCharacters[playersConnectionId].battlePrefab));

		//add more scene types here

		throw new System.Exception("Scene not reconized, please add your scene to a list in the CustomNetworkManager.");
	}
}
