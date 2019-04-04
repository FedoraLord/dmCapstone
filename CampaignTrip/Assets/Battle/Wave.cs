using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyPrefab;

[Serializable]
public class Wave
{
    public List<EnemyType> members;

    public List<GameObject> GetEnemyPrefabs(EnemyDataList data)
    {
        List<GameObject> prefabs = new List<GameObject>();
        foreach (EnemyType type in members)
        {
            prefabs.Add(data.GetPrefab(type));
        }
        return prefabs;
    }
}