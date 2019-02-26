using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour
{
    public MinigameManager MinigameManagerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Command]
    public void CmdSpawnMinigameManager()
    {
        Instantiate(MinigameManagerPrefab);
    }
}
