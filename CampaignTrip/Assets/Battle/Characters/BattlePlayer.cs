using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BattlePlayer : NetworkBehaviour
{
    [SyncVar(hook = "UpdateHealthUI")]
    public int health;

    public PersistentPlayer persistentPlayer;

    public void Initialize()
    {
        int i = persistentPlayer.playerNum - 1;

        Camera cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector3[] corners = new Vector3[4];
        BattleController.Instance.spawnPoints[i].GetWorldCorners(corners);

        Vector3 camPos = corners[0] + corners[1] + corners[2] + corners[3];
        camPos /= 4;

        Vector3 worldPosition = cam.ScreenToWorldPoint(camPos);
        worldPosition.z = 0;
        transform.position = worldPosition;
    }

    private void UpdateHealthUI()
    {

    }
}
