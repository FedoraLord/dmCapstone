using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class CustomNetworkManager : NetworkManager
{
	public string sceneAfterLobbyName;

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		GameObject player = (GameObject)Instantiate(CharacterCreator.instance.chosenCharacters[conn.connectionId].battlePrefab);
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
	}
}
#pragma warning restore CS0618