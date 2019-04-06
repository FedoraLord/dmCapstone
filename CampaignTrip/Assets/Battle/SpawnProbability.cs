using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EnemyBase;

[Serializable]
public class SpawnProbability
{
    public EnemyType enemy;
    public float probability;
}

[Serializable]
public class SpawnGroup
{
    public int WaveIndex { get; set; }
    public int NumWaves { get; set; }
    public float SumProbability
    {
        get
        {
            if (sumProbability == -1)
            {
                sumProbability = 0;
                foreach (SpawnProbability sp in group)
                    sumProbability += sp.probability;
            }
            return sumProbability;
        }
    }
    
    public List<SpawnProbability> group = new List<SpawnProbability>();

    private float sumProbability = -1;

    public float GetProbability(EnemyType type)
    {
        foreach (SpawnProbability spawn in group)
        {
            if (spawn.enemy == type)
                return spawn.probability / SumProbability;
        }
        return 0;
    }

    public List<EnemyType> GetEnemies()
    {
        List<EnemyType> enemies = new List<EnemyType>();
        foreach (SpawnProbability spawn in group)
            enemies.Add(spawn.enemy);
        return enemies;
    }
    
    public static SpawnGroup Average(SpawnGroup prev, SpawnGroup next, int currentWave)
    {
        if (prev.WaveIndex > next.WaveIndex)
        {
            Debug.LogError("SpawnGroup passed out of order.");
            return prev;
        }

        if (prev.WaveIndex == currentWave)
            return prev;
        else if (next.WaveIndex == currentWave)
            return next;

        float distFromFirst = currentWave - prev.WaveIndex;
        float nextWeight = distFromFirst / (next.WaveIndex - prev.WaveIndex);
        float prevWeight = 1 - nextWeight;

        if (prevWeight < 0 || nextWeight < 0 || prevWeight > 1 || nextWeight > 1)
        {
            Debug.LogError("Im bad at math");
            return prev;
        }

        SpawnGroup avg = new SpawnGroup();
        List<EnemyType> enemies = prev.GetEnemies().Union(next.GetEnemies()).ToList();
        foreach (EnemyType type in enemies)
        {
            float probability = prev.GetProbability(type) * prevWeight + next.GetProbability(type) * nextWeight;
            avg.group.Add(new SpawnProbability()
            {
                enemy = type,
                probability = probability
            });
        }
        
        return avg;
    }

    public EnemyType ChooseRandom()
    {
        float total = 0;
        float rand = UnityEngine.Random.Range(0f, 1f);
        for (int i = 0; i < group.Count; i++)
        {
            total += group[i].probability / SumProbability;
            if (rand < total || i == group.Count - 1)
                return group[i].enemy;
        }
        return EnemyType.None;
    }
}