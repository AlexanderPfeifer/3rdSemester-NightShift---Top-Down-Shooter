using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Ride : MonoBehaviour
{
    [SerializeField] private GameObject enemyMaderPrefab;
    [SerializeField] private GameObject enemyBunnyPrefab;
    [SerializeField] private GameObject enemyRacoonPrefab;
    [SerializeField] private List<SpawnPoint> spawnPoints;
    [SerializeField] public GameObject rideLight;

    private static int rideCount;
        
    [SerializeField] private RidesSO rideData;

    [SerializeField] private List<GameObject> invisibleCollider;
    [SerializeField] private List<int> enemySpawnPointCount;
    [SerializeField] private List<GameObject> enemyList;
    
    [SerializeField] private int waveCount;

    [SerializeField] private bool waveStarted;

    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private float maxTimeBetweenSpawns = 20;
    [SerializeField] public float currentWaveTimer;
    [SerializeField] private float maxWaveTimer = 120f;

    public bool rideIsRunning;
    public bool canActivateRide;

    [SerializeField] public CinemachineVirtualCamera fightCam;

    private Color noAlpha;
    public float currentRideHealth;
    [SerializeField] private float maxRideHealth = 50;

    private void Awake()
    {
        //When loading the scene, we destroy the collectible, if it was already saved as collected.
        if (GameSaveStateManager.instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            rideIsRunning = true;
            rideCount++;
        }
    }
    
    private void Start()
    {
        currentWaveTimer = 0;
        
        ActivateInvisibleWalls(false);
    }

    private void Update()
    {
        TimerUpdate();
    }

    public void StartWave()
    {
        InGameUI.instance.fightScene.SetActive(true);
        canActivateRide = false;
        currentRideHealth = maxRideHealth;

        if (rideCount == 0)
        {
            StartFirstWave();
        }
        else if (rideCount >= 0)
        {
            StartSecondWave();
        }
        else if(rideCount >= 4)
        {
            StartThirdWave();
        }
    }
    
    #region Waves
    private void StartFirstWave()
    {
        if (!rideIsRunning)
        {
            timeBetweenSpawns = maxTimeBetweenSpawns;

            switch (waveCount)
            {
                case 0 :
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    break;
                case 1 :
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[1]);
                    break;
                case 2 :
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 3, enemySpawnPointCount[1]);
                    break;
                case 3 :
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[1]);
                    break;
                case 4:
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[1]);
                    break;
                case 5:
                    InstantiateEnemies(enemyMaderPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[1]);
                    break;
            }

            waveStarted = true;
            
            waveCount += 1;
        }
    }
    
    private void StartSecondWave()
    {
        if (!rideIsRunning)
        {
            for (int i = 0; i < invisibleCollider.Count; i++)
            {
                invisibleCollider[i].SetActive(true);
            }
            
            timeBetweenSpawns = maxTimeBetweenSpawns;

            switch (waveCount)
            {
                case 0 :
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    break;
                case 1 :
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[1]);
                    break;
                case 2 :
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 3, enemySpawnPointCount[1]);
                    break;
                case 3 :
                    currentWaveTimer = maxWaveTimer;
                    break;
            }

            waveStarted = true;
            
            waveCount += 1;
        }
    }
    
    private void StartThirdWave()
    {
        if (!rideIsRunning)
        {
            for (int i = 0; i < invisibleCollider.Count; i++)
            {
                invisibleCollider[i].SetActive(true);
            }
            
            timeBetweenSpawns = maxTimeBetweenSpawns;

            switch (waveCount)
            {
                case 0 :
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    break;
                case 1 :
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[1]);
                    break;
                case 2 :
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 3, enemySpawnPointCount[1]);
                    break;
                case 3 :
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[1]);
                    break;
                case 4:
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[2]);
                    InstantiateEnemies(enemyBunnyPrefab, 2, enemySpawnPointCount[1]);
                    break;
                case 5:
                    InstantiateEnemies(enemyMaderPrefab, 1, enemySpawnPointCount[0]);
                    InstantiateEnemies(enemyBunnyPrefab, 1, enemySpawnPointCount[1]);
                    break;
            }

            waveStarted = true;
            
            waveCount += 1;
        }
    }
    #endregion

    private void TimerUpdate()
    {
        if (!rideIsRunning && waveStarted)
        {
            Slider slider = InGameUI.instance.rideHpSlider.GetComponentInChildren<Slider>();

            slider.value = currentRideHealth / maxRideHealth;

            Slider timeSlider = InGameUI.instance.rideTimeSlider.GetComponent<Slider>();

            timeSlider.value = currentWaveTimer / maxWaveTimer;
            
            currentWaveTimer += Time.deltaTime;
            
            timeBetweenSpawns -= Time.deltaTime;

            if (timeBetweenSpawns <= 0 || !FindObjectOfType<Enemy>())
            {
                StartFirstWave();
            }
            
            if (currentWaveTimer >= 120 && !GameSaveStateManager.instance.saveGameDataManager.HasFinishedRide(rideData.rideName) && !GetComponentInChildren<Generator>().arenaFightFinished)
            {
                RideRepairs();
            }

            if (currentRideHealth <= 0)
            {
                ResetRide();
            }
        }
    }
    
    public void ActivateInvisibleWalls(bool activationStatus)
    {
        for (int i = 0; i < invisibleCollider.Count; i++)
        {
            invisibleCollider[i].SetActive(activationStatus);
        }
    }

    private void InstantiateEnemies(GameObject enemyType, int enemies, int enemySpawnPoint)
    {
        for (int i = 0; i < enemies; i++)
        {
            var enemy = Instantiate(enemyType, spawnPoints[enemySpawnPoint].transform.position, Quaternion.identity, transform);
            enemyList.Add(enemy);
        }
    }

    public IEnumerator ChangeAlphaOnAttack()
    {
        SpriteRenderer rideRenderer = GetComponent<SpriteRenderer>();
        
        noAlpha.r = 1;
        noAlpha.g = 0;
        noAlpha.b = 0;
        noAlpha.a = 0.3f;
        rideRenderer.color = noAlpha;
        yield return new WaitForSeconds(0.05f);
        noAlpha.r = 1;
        noAlpha.g = 1;
        noAlpha.b = 1;
        noAlpha.a = 1f;
        rideRenderer.color = noAlpha;
    }
    
    public void ResetRide()
    {
        currentWaveTimer = 0;

        waveCount = 0;
        
        for (int i = 0; i < enemyList.Count; i++)
        {
            var enemy = enemyList[0].gameObject;
            Destroy(enemy);
        }
        
        canActivateRide = true;
    }

    private void RideRepairs()
    {
        for (int i = 0; i < invisibleCollider.Count; i++)
        {
            invisibleCollider[i].SetActive(false);
        }
        
        InGameUI.instance.rideHpSlider.SetActive(false);
        
        InGameUI.instance.radioAnim.SetTrigger("PutOn");

        for (int i = 0; i < enemyList.Count; i++)
        {
            var enemy = enemyList[0].gameObject;
            Destroy(enemy);
        }
        
        InGameUI.instance.fightScene.SetActive(false);

        FindObjectOfType<Player>().currentAbilityProgress = 0;
        
        fightCam.Priority = 5;
        
        rideCount++;

        GameSaveStateManager.instance.saveGameDataManager.AddRide(rideData.rideName);
        
        GameSaveStateManager.instance.SaveGame();
        
        GetComponentInChildren<Generator>().arenaFightFinished = true;
    }
}
