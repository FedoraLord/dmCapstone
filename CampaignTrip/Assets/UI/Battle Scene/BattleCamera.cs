using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649
public class BattleCamera : MonoBehaviour
{
    public Camera Cam { get { return cam; } }
    public List<Transform> EnemySpawnPoints { get { return enemySpawnPoints; } }
    public List<Transform> PlayerSpawnPoints { get { return playerSpawnPoints; } }

    [SerializeField] private Camera cam;
    [SerializeField] private List<Transform> enemySpawnPoints;
    [SerializeField] private List<Transform> playerSpawnPoints;
}
