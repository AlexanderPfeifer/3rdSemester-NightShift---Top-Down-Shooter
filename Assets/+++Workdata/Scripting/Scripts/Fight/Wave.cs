using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyClusterData
{
    [FormerlySerializedAs("clusterBeginTime")] [SerializeField, ReadOnly] private string clusterName;

    [Header("WHO")]
    public GameObject enemyPrefab;
    
    [FormerlySerializedAs("timeToSpawn")]
    [Header("WHEN")]
    [Min(0)] public float spawnStartTime;
    
    [Header("HOW MANY")]
    [Min(1)] public int spawnCount;
    
    [Header("HOW MANY REPETITIONS")]
    [Min(1)] public int repeatCount;
    
    [Header("AT WHAT INTERVAL")]
    [Min(0)] public float timeBetweenSpawns;
    
    [Header("UNTIL")]
    [SerializeField, ReadOnly] private float stopsSpawningAtTime;

    public void UpdateClusterName()
    {
        clusterName = enemyPrefab.name + " | " + spawnStartTime.ToString(CultureInfo.CurrentCulture) + " -> " + (repeatCount * timeBetweenSpawns + spawnStartTime);

        stopsSpawningAtTime = repeatCount * timeBetweenSpawns + spawnStartTime;
    }
}

[Serializable]
public class Wave
{
    [ReadOnly] public string waveName;
    
    public int currencyPrize;

    [Min(1)] public float maxWaveTime = 120f;

    public List<EnemyClusterData> enemyClusters = new();
}
