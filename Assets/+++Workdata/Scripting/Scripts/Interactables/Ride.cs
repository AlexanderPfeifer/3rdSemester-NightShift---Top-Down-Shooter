using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Ride : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private GameObject enemyMaderPrefab;
    [SerializeField] private GameObject enemyBunnyPrefab;
    [SerializeField] private GameObject enemyRacoonPrefab;
    [SerializeField] private GameObject bigMaderPrefab;
    [SerializeField] private GameObject bigBunnyPrefab;
    [SerializeField] private GameObject bigRacoonPrefab;
    
    [Header("Spawning")]
    [SerializeField] private List<SpawnPoint> spawnPoints;
    private readonly List<GameObject> currentEnemies = new();
    [SerializeField] private float maxTimeBetweenSpawns = 20;
    private float currentTimeBetweenSpawns;
    private bool enemyHasSpawned;
    
    [Header("Ride")]
    [SerializeField] private RidesSO rideData;
    [SerializeField] private float maxRideHealth = 50;
    public GameObject rideLight;
    [HideInInspector] public bool canActivateRide;
    [HideInInspector] public float currentRideHealth;
    private bool rideGotHit;
    private bool rideGotDestroyed;
    
    [Header("Arena")]
    [SerializeField] private List<GameObject> invisibleCollider;
    [HideInInspector] public CinemachineCamera fightCam;

    [Header("Wave")]
    [SerializeField] private float maxWaveTimer = 120f;
    private bool waveStarted;
    private int waveCount;
    private float currentWaveTimer;
    private float currentTime;
    private float currentTime2;
    private float currentTime3;
    
    private void Start()
    {
        if (GameSaveStateManager.Instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            //Ride and dialogue count goes up, so the game knows which wave and which dialogue has to be played
            InGameUIManager.Instance.dialogueCount++;
            Player.Instance.enemyWave++;
            GetComponent<Animator>().SetTrigger("StartRide");
            GetComponent<Animator>().SetTrigger("LightOn");
        }
        
        currentTimeBetweenSpawns = maxTimeBetweenSpawns;

        fightCam = GetComponentInChildren<CinemachineCamera>();
        ActivationStatusInvisibleWalls(false);
    }

    private void Update()
    {
        TimerUpdate();

        CheckAndStartWaveUpdate();
    }
    
    private void TimerUpdate()
    {
        if (!waveStarted) 
            return;
        
        InGameUIManager.Instance.rideHpSlider.GetComponentInChildren<Slider>().value = currentRideHealth / maxRideHealth;
        InGameUIManager.Instance.rideTimeSlider.GetComponent<Slider>().value = currentWaveTimer / maxWaveTimer;
        currentWaveTimer += Time.deltaTime;
        currentTimeBetweenSpawns -= Time.deltaTime;
            
        if (currentTimeBetweenSpawns <= 0)
        {
            waveCount++;
            currentTimeBetweenSpawns = maxTimeBetweenSpawns;
            currentTime = 0;
            currentTime2 = 0;
            currentTime3 = 0;
        }
            
        if (currentWaveTimer >= 120 && 
            !GameSaveStateManager.Instance.saveGameDataManager.HasFinishedRide(rideData.rideName) && 
            !GetComponentInChildren<Generator>().arenaFightFinished)
        {
            WonWave();
        }
        else if (currentRideHealth <= 0)
        {
            LostWave();
        }
    }
    
    public void ActivationStatusInvisibleWalls(bool activationStatus)
    {
        foreach (var _invisibleWalls in invisibleCollider)
        {
            _invisibleWalls.SetActive(activationStatus);
        }
    }

    #region WaveSetup

    public void SetWave()
    {
        InGameUIManager.Instance.fightScene.SetActive(true);
        SetRide(false, true);
        
        //Check if rideGotDestroyed because at the start the music already plays when turning on the generator
        if (rideGotDestroyed)
            GetComponentInChildren<Generator>().fightMusic.Play();
    }

    private void CheckAndStartWaveUpdate()
    {
        if (waveStarted)
        {
            switch (Player.Instance.enemyWave)
            {
                case 0:
                    StartFirstWave();
                    break;
                case 1:
                    StartSecondWave();
                    break;
                case 2:
                    StartThirdWave();
                    break;
            }
        }
    }
    
    private void LostWave()
    {
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicLoss");
        
        SetRide(true, false);
        rideGotDestroyed = true;
        
        foreach (var _enemy in currentEnemies)
        {
            Destroy(_enemy);
        }
    }

    private void WonWave()
    {
        Player.Instance.enemyWave++;

        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicWon");
        GetComponent<Animator>().SetTrigger("StartRide");
        
        ActivationStatusInvisibleWalls(false);

        foreach (var _enemy in currentEnemies)
        {
            Destroy(_enemy);
        }
        
        GetComponentInChildren<PlayerArenaEnter>().canPutAwayWalkieTalkie = true;
        currentTimeBetweenSpawns = maxTimeBetweenSpawns;
        waveStarted = false;
        fightCam.Priority = 5;
        InGameUIManager.Instance.fightScene.SetActive(false);
        InGameUIManager.Instance.radioAnim.SetTrigger("PutOn");
        StartCoroutine(InGameUIManager.Instance.DisplayDialogueElements());
        GetComponentInChildren<Generator>().arenaFightFinished = true;
        GameSaveStateManager.Instance.saveGameDataManager.AddRide(rideData.rideName);
        GameSaveStateManager.Instance.SaveGame();
        AudioManager.Instance.Play("InGameMusic");
    }

    #endregion
    
    #region enemyWaves

    private void StartFirstWave()
    {
        switch (waveCount)
        {
            case 0 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 2);
                    currentTime = 3;
                }
                break;
            case 1 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 4);
                    currentTime = 4.5f;
                }
                break;
            case 2 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 3);
                    currentTime = 4;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 2);
                    currentTime2 = 2;
                }
                break;
            case 3 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 6);
                    currentTime = 3;
                }
                break;
            case 4:
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 5);
                    currentTime = 6;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 3);
                    currentTime2 = 4;
                }
                
                currentTime3 -= Time.deltaTime;
                if (currentTime3 < 0)
                {
                    SpawnEnemies(bigBunnyPrefab, 1);
                    currentTime3 = 25;
                }
                break;
            case 5:
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 2);
                    currentTime = 3.5f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(bigBunnyPrefab, 2);
                    currentTime2 = 25;
                }
                break;
        }
    }

    private void StartSecondWave()
    {
        switch (waveCount)
        {
            case 0 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 4);
                    currentTime = 2.5f;
                }
                break;
            case 1 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 3);
                    currentTime = 3.5f;
                }
                break;
            case 2 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyRacoonPrefab, 1);
                    currentTime = 4;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 4);                    
                    currentTime2 = 4;
                }
                break;
            case 3 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(bigBunnyPrefab, 1);
                    currentTime = 25f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 2);
                    currentTime2 = 5f;
                }
                break;
            case 4:
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(bigMaderPrefab, 1);
                    currentTime = 25f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 2);
                    currentTime2 = 3.5f;
                }
                break;
            case 5:
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyRacoonPrefab, 3);
                    currentTime = 5f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 3);
                    currentTime2 = 4f;
                }
                break;
        }
    }
    
    private void StartThirdWave()
    {
        switch (waveCount)
        {
            case 0 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyRacoonPrefab, 2);
                    currentTime = 3f;
                }
                break;
            case 1 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 2);
                    currentTime = 2.5f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyRacoonPrefab, 1);
                    currentTime2 = 5f;
                }
                break;
            case 2 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 10);
                    currentTime = 7.5f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(enemyRacoonPrefab, 1);
                    currentTime2 = 6f;
                }
                break;
            case 3 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyMaderPrefab, 5);
                    currentTime = 2.5f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(bigMaderPrefab, 1);
                    currentTime2 = 25f;
                }
                break;
            case 4 :
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyRacoonPrefab, 3);
                    currentTime = 5f;
                }

                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(bigRacoonPrefab, 1);
                    currentTime2 = 25f;
                }
                break;
            case 5:
                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    SpawnEnemies(enemyBunnyPrefab, 5);
                    currentTime = 5f;
                }
                
                currentTime2 -= Time.deltaTime;
                if (currentTime2 < 0)
                {
                    SpawnEnemies(bigBunnyPrefab, 1);
                    currentTime2 = 10;
                }
                break;
        }
    }
    
    private void SpawnEnemies(GameObject enemyType, int enemyCount)
    {
        for (int _i = 0; _i < enemyCount; _i++)
        {
            var _enemy = Instantiate(enemyType, spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position, 
                Quaternion.identity, transform); 
            
            currentEnemies.Add(_enemy);
        }
    }

    #endregion
    
    #region Ride
    
    private void SetRide(bool setRideActivation, bool waveStart)
    {
        currentWaveTimer = 0;
        waveCount = 0;
        currentRideHealth = maxRideHealth;
        canActivateRide = setRideActivation;
        waveStarted = waveStart;
    }

    public void StartRideHitVisual(float duration)
    {
        if (rideGotHit)
            return;

        rideGotHit = true;

        SpriteRenderer _rideRenderer = GetComponent<SpriteRenderer>();
        _rideRenderer.color = Color.red;

        Time.timeScale = 0.1f;
        
        StartCoroutine(StopRideHitVisual(duration, _rideRenderer));
    }
    
    private IEnumerator StopRideHitVisual(float duration, SpriteRenderer rideRenderer)
    {
        yield return new WaitForSecondsRealtime(duration);
        
        Time.timeScale = 1f;
        
        rideRenderer.color = Color.white;
        
        rideGotHit = false;
    }

    #endregion
}
