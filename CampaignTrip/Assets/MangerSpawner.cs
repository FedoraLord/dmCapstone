using UnityEngine;

public class MangerSpawner : MonoBehaviour
{
    public GameObject minigameManager;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkWrapper.IsHost)
        {
            var manager = Instantiate(minigameManager);
            NetworkSpawner.Instance.NetworkSpawn(manager);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
