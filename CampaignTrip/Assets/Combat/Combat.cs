using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public static Combat Instance;

    public List<RectTransform> spawnPoints;

    void Start()
    {
        Instance = this;
        Player.localAuthority.CmdSpawnCharacter();
    }
}
