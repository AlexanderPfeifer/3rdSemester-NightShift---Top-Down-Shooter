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
    
    [Header("WHEN")]
    [Min(0)] public float timeToSpawn;
    
    [Header("HOW MANY")]
    [Min(1)] public int spawnCount;
    
    [Header("HOW MANY REPETITIONS")]
    [Min(0)] public int repeatCount;
    
    [Header("AT WHAT INTERVAL")]
    [Min(0)] public float timeBetweenSpawns;
    
    public void UpdateClusterName()
    {
        clusterName = enemyPrefab.name + " | " +timeToSpawn.ToString(CultureInfo.CurrentCulture);
    }
}

[Serializable]
public class Wave
{
    [ReadOnly] public string waveName;

    [Min(1)] public float maxWaveTime = 120f;

    public List<EnemyClusterData> enemyClusters = new();
}
