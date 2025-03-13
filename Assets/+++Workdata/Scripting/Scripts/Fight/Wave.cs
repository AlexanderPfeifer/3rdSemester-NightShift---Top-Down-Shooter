using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyClusterData
{
    [Header("WHO")]
    public GameObject enemyPrefab;
    
    [Header("WHEN")]
    [Min(0)] public float timeToSpawn;
    
    [Header("HOW MANY")]
    [Min(0)] public int spawnCount;
    
    [Header("HOW MANY REPETITIONS")]
    [Min(0)] public int repeatCount;
    
    [Header("AT WHAT INTERVAL")]
    [Min(0)] public float timeBetweenSpawns;
    
    public void ValidateValues()
    {
        spawnCount = Mathf.Max(1, spawnCount);
    }
}

[Serializable]
public class Wave
{
    [Min(1)] public float maxWaveTime = 120f;

    public List<EnemyClusterData> enemyClusters = new();

    public void ValidateValues()
    {
        maxWaveTime = Mathf.Max(1, maxWaveTime);
    }
}
