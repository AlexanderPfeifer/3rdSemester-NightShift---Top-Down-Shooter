using System.Collections.Generic;
using UnityEngine;

public class Ride : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<SpawnPoint> spawnPoints;
        
    [SerializeField] private RidesSO rideData;

    [SerializeField] private List<GameObject> invisibleCollider;
    [SerializeField] private List<int> enemySpawnPointCount;
    [SerializeField] private List<GameObject> enemyList;
    
    [SerializeField] private int waveCount;

    [SerializeField] private bool waveStarted;

    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private float maxTimeBetweenSpawns = 20;
    [SerializeField] private float waveTimer = 120f;

    private bool rideIsActive;

    public float currentRideHp;
    [SerializeField] private float maxRideHp;

    private void Awake()
    {
        //When loading the scene, we destroy the collectible, if it was already saved as collected.
        if (GameSaveStateManager.instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
            rideIsActive = true;
    }
    
    private void Start()
    {
        currentRideHp = maxRideHp;
        
        for (int i = 0; i < invisibleCollider.Count; i++)
        {
            invisibleCollider[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (!rideIsActive && waveStarted)
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
    }

    public void StartNextWave()
    {
        if (!rideIsActive)
        {
            for (int i = 0; i < invisibleCollider.Count; i++)
            {
                invisibleCollider[i].SetActive(true);
            }
            
            timeBetweenSpawns = maxTimeBetweenSpawns;

            switch (waveCount)
            {
                case 0 :
                    InstantiateEnemies(2, enemySpawnPointCount[0]);
                    break;
                case 1 :
                    InstantiateEnemies(2, enemySpawnPointCount[0]);
                    InstantiateEnemies(2, enemySpawnPointCount[1]);
                    break;
                case 2 :
                    InstantiateEnemies(1, enemySpawnPointCount[0]);
                    InstantiateEnemies(1, enemySpawnPointCount[2]);
                    InstantiateEnemies(3, enemySpawnPointCount[1]);
                    break;
                case 3 :
                    waveTimer = 0;
                    break;
            }

            waveStarted = true;
            
            waveCount += 1;
        }
    }

    private void InstantiateEnemies(int enemies, int enemySpawnPoint)
    {
        for (int i = 0; i < enemies; i++)
        {
            var enemy = Instantiate(enemyPrefab, spawnPoints[enemySpawnPoint].transform.position, Quaternion.identity, transform);
            enemyList.Add(enemy);
        }
    }

    public void ResetRide()
    {
        currentRideHp = maxRideHp;

        waveCount = 0;
        
        for (int i = 0; i < enemyList.Count; i++)
        {
            var enemy = enemyList[0].gameObject;
            Destroy(enemy);
        }
    }

    private void RideRepairs()
    {
        for (int i = 0; i < invisibleCollider.Count; i++)
        {
            invisibleCollider[i].SetActive(false);
        }
        
        for (int i = 0; i < enemyList.Count; i++)
        {
            var enemy = enemyList[0].gameObject;
            Destroy(enemy);
        }
        
        GameSaveStateManager.instance.saveGameDataManager.AddRide(rideData.rideName);
        
        GameSaveStateManager.instance.SaveGame();
    }
}
