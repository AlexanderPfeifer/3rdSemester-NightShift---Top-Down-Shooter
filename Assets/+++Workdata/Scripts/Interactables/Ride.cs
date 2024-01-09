using System;
using System.Collections.Generic;
using UnityEngine;

public class Ride : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<SpawnPoint> spawnPoints;
        
    [SerializeField] private List<int> enemyCount;
    [SerializeField] private List<int> enemySpawnPointCount;
    
    [SerializeField] private int waveCount;

    [SerializeField] private float timeBetweenSpawns = 20;
    [SerializeField] private float waveTimer = 120f;

    public float currentRideHp;
    [SerializeField] private float maxRideHp;

    private void Start()
    {
        currentRideHp = maxRideHp;
    }

    private void Update()
    {
        waveTimer -= Time.deltaTime;
        timeBetweenSpawns -= Time.deltaTime;

        if (timeBetweenSpawns <= 0 || !FindObjectOfType<Enemy>())
        {
            StartNextWave();
        }

        if (waveTimer <= 0)
        {
            RideRepairs();
        }
    }

    public void StartNextWave()
    {
        switch (waveCount)
        {
            case 0 :
                InstantiateEnemies(enemyCount[2], enemySpawnPointCount[0]);
                break;
            case 1 :
                InstantiateEnemies(enemyCount[2], enemySpawnPointCount[0]);
                InstantiateEnemies(enemyCount[2], enemySpawnPointCount[3]);
                break;
            case 2 :
                InstantiateEnemies(enemyCount[1], enemySpawnPointCount[0]);
                InstantiateEnemies(enemyCount[1], enemySpawnPointCount[3]);
                InstantiateEnemies(enemyCount[3], enemySpawnPointCount[2]);
                break;
        }

        waveCount += 1;
    }

    private void InstantiateEnemies(int enemies, int enemySpawnPoint)
    {
        for (int i = 0; i < enemies; i++)
        {
            Instantiate(enemyPrefab, spawnPoints[enemySpawnPoint].transform.position, Quaternion.identity, transform);
        }
    }

    public void ResetRide()
    {
        currentRideHp = maxRideHp;
        for (int i = 0; i < transform.childCount; i++)
        {
            var enemy = transform.GetChild(0).transform.gameObject;
            Destroy(enemy);
        }
    }

    private void RideRepairs()
    {
        GameSaveStateManager.instance.SaveGame();
        
    }
}
