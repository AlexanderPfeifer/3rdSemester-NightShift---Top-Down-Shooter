using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Ride : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject enemyMaderPrefab;
    [SerializeField] private GameObject bigMaderPrefab;
    [SerializeField] private GameObject enemyBunnyPrefab;
    [SerializeField] private GameObject bigBunnyPrefab;
    [SerializeField] private GameObject enemyRacoonPrefab;
    [SerializeField] private GameObject bigRacoonPrefab;
    [SerializeField] private List<SpawnPoint> spawnPoints;
    [SerializeField] public GameObject rideLight;
    [SerializeField] private RidesSO rideData;
    [SerializeField] private List<GameObject> invisibleCollider;
    [SerializeField] private List<GameObject> enemyList;
    [HideInInspector] public CinemachineCamera fightCam;
    private Color noAlpha;

    [Header("Boolean")]
    private bool rideGotHit;
    [SerializeField] private bool waveStarted;
    private bool enemyHasSpawned;
    private bool rideGotDestroyed;
    public bool canActivateRide;

    [Header("Ints")]
    [SerializeField] private int waveCount;

    [Header("Floats")]
    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private float maxTimeBetweenSpawns = 20;
    [SerializeField] public float currentWaveTimer;
    [SerializeField] private float maxWaveTimer = 120f;
    public float currentRideHealth;
    [SerializeField] private float maxRideHealth = 50;
    private float currentTime;
    private float currentTime2;
    private float currentTime3;

    //If Ride was already finished, it turns on. Ride and dialogue count goes up, so the game knows which wave and which dialogue has to be played
    private void Start()
    {
        if (GameSaveStateManager.Instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            canActivateRide = false;
            InGameUI.Instance.dialogueCount++;
            GetComponent<Animator>().SetTrigger("LightOn");
            GetComponent<Animator>().SetTrigger("StartRide");
            Player.Instance.rideCount++;
        }

        fightCam = GetComponentInChildren<CinemachineCamera>();
        
        ActivateInvisibleWalls(false);
    }

    //Updates everything time specific
    private void Update()
    {
        TimerUpdate();

        if (waveStarted)
        {
            if (Player.Instance.rideCount == 0)
            {
                StartFirstWave();
            }
            else if (Player.Instance.rideCount == 1)
            {
                StartSecondWave();
            }
            else if(Player.Instance.rideCount == 2)
            {
                StartThirdWave();
            }
        }
    }

    //Starts the waves and its ui. Also sets timer and health of ride accordingly
    public void StartWave()
    {
        InGameUI.Instance.fightScene.SetActive(true);
        SetRide(false, true);
        
        if (rideGotDestroyed)
        {
            GetComponentInChildren<Generator>().fightMusic.Play();
        }
    }

    private void SetRide(bool setRideActivation, bool waveStart)
    {
        currentWaveTimer = 0;
        waveCount = 0;
        currentRideHealth = maxRideHealth;
        canActivateRide = setRideActivation;
        waveStarted = waveStart;
    }
    
    #region Waves
    //The first waves with its enemies and spawn times
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

    //The second waves with its enemies and spawn times
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
    
    //The third waves with its enemies and spawn times
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
    #endregion

    //Sets the spawn times of enemies to zero
    private void SetCurrentTimeZero()
    {
        currentTime = 0;
        currentTime2 = 0;
        currentTime3 = 0;
    }
    
    //Here every slider and timer is updated. When wave timer hits its limit the player wins, if hp of the ride hits zero, the ride resets
    private void TimerUpdate()
    {
        if (!waveStarted) 
            return;
        
        Slider slider = InGameUI.Instance.rideHpSlider.GetComponentInChildren<Slider>();
        slider.value = currentRideHealth / maxRideHealth;
        Slider timeSlider = InGameUI.Instance.rideTimeSlider.GetComponent<Slider>();
        timeSlider.value = currentWaveTimer / maxWaveTimer;
        currentWaveTimer += Time.deltaTime;
        timeBetweenSpawns -= Time.deltaTime;
            
        if (timeBetweenSpawns <= 0)
        {
            waveCount++;
            timeBetweenSpawns = maxTimeBetweenSpawns;
            SetCurrentTimeZero();
        }
            
        if (currentWaveTimer >= 120 && !GameSaveStateManager.Instance.saveGameDataManager.HasFinishedRide(rideData.rideName) && !GetComponentInChildren<Generator>().arenaFightFinished)
        {
            RideRepairs();
        }

        if (currentRideHealth <= 0)
        {
            ResetRide();
        }
    }
    
    //Activates or deactivates invisible walls for the player
    public void ActivateInvisibleWalls(bool activationStatus)
    {
        foreach (var invisibleWalls in invisibleCollider)
        {
            invisibleWalls.SetActive(activationStatus);
        }
    }

    //Spawns a specific number of a specific enemy type on a random spawn point
    private void SpawnEnemies(GameObject enemyType, int enemies)
    {
        for (int i = 0; i < enemies; i++)
        {
            var enemy = Instantiate(enemyType, spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position, Quaternion.identity, transform);
            enemyList.Add(enemy);
        }
    }

    //When ride got hit, it changes color 
    public void HitRide(float duration)
    {
        if (rideGotHit)
        {
            return;
        }
        
        noAlpha.r = 1;
        noAlpha.g = 0;
        noAlpha.b = 0;
        noAlpha.a = 0.3f;
        GetComponent<SpriteRenderer>().color = noAlpha;
        
        Time.timeScale = 0;

        StartCoroutine(RideGotHit(duration));
    }
    
    //Here it changes color to normal
    private IEnumerator RideGotHit(float duration)
    {
        rideGotHit = true;

        yield return new WaitForSecondsRealtime(duration);
        
        Time.timeScale = 1f;

        noAlpha.r = 1;
        noAlpha.g = 1;
        noAlpha.b = 1;
        noAlpha.a = 1;
        GetComponent<SpriteRenderer>().color = noAlpha;
        rideGotHit = false;
    }

    //Resets ride when enemies get the hp to zero, can be activated by player then
    private void ResetRide()
    {
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("RideLoose");
        
        SetRide(true, false);
        rideGotDestroyed = true;
        
        foreach (var _objects in enemyList)
        {
            Destroy(_objects);
        }
    }

    //Starts everything that is needed to end the ride and get back to normal
    private void RideRepairs()
    {
        Player.Instance.isInteracting = true;
        Player.Instance.rideCount++;
        Player.Instance.isInteracting = false;

        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicEnd");
        GetComponent<Animator>().SetTrigger("StartRide");
        
        ActivateInvisibleWalls(false);

        foreach (var objects in enemyList)
        {
            Destroy(objects);
        }

        GetComponentInChildren<PlayerCollision>().canPutAway = true;
        timeBetweenSpawns = maxTimeBetweenSpawns;
        waveStarted = false;
        fightCam.Priority = 5;
        InGameUI.Instance.fightScene.SetActive(false);
        InGameUI.Instance.radioAnim.SetTrigger("PutOn");
        StartCoroutine(InGameUI.Instance.DisplayDialogueElements());
        GameSaveStateManager.Instance.saveGameDataManager.AddRide(rideData.rideName);
        GameSaveStateManager.Instance.SaveGame();
        GetComponentInChildren<Generator>().arenaFightFinished = true;
        AudioManager.Instance.Play("InGameMusic");
    }
}
