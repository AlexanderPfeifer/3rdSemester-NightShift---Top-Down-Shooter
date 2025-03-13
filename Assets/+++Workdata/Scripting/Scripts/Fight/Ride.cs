using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ride : MonoBehaviour
{
    [Header("Waves")] 
    public Wave[] waves;
    private bool waveStarted;
    private float waveTimer;

    [Header("Spawning")]
    [SerializeField] private List<SpawnPoint> spawnPoints;
    [SerializeField] private float maxTimeBetweenSpawns = 20;
    [SerializeField] private GameObject enemyParent;
    private readonly List<GameObject> currentEnemies = new();
    private float currentTimeBetweenSpawns;
    private bool enemyHasSpawned;
    
    [Header("Health")]
    [SerializeField] private float maxRideHealth = 50;
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private ParticleSystem hitParticles;
    [HideInInspector] public float currentRideHealth;
    private bool rideGotHit;
    private bool rideGotDestroyed;

    [Header("Activation")]
    public GameObject rideLight;
    
    [Header("Arena")]
    [SerializeField] private GameObject invisibleCollider;

    private void Start()
    {
        InGameUIManager.Instance.dialogueCount = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
        
        currentTimeBetweenSpawns = maxTimeBetweenSpawns;

        ActivationStatusInvisibleWalls(false);
    }

    private void Update()
    {
        TimerUpdate();
    }
    
    private void TimerUpdate()
    {
        if (!waveStarted) 
            return;
        
        InGameUIManager.Instance.rideHpSlider.GetComponentInChildren<Slider>().value = currentRideHealth / maxRideHealth;
        //InGameUIManager.Instance.rideTimeSlider.GetComponent<Slider>().value = currentWaveTimer / maxWaveTimer;
        waveTimer += Time.deltaTime;
        currentTimeBetweenSpawns -= Time.deltaTime;
            
        if (currentTimeBetweenSpawns <= 0)
        {
            currentTimeBetweenSpawns = maxTimeBetweenSpawns;
        }
            
        if (waveTimer >= waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()].maxWaveTime)
        {
            WonWave();
        }
        
        if (currentRideHealth <= 0)
        {
            LostWave();
        }
    }
    
    public void ActivationStatusInvisibleWalls(bool activationStatus)
    {
        invisibleCollider.SetActive(activationStatus);
    }

    #region WaveSetup

    public void SetWave()
    {
        InGameUIManager.Instance.fightScene.SetActive(true);
        SetRide();
        
        //Check if rideGotDestroyed because at the start the music already plays when turning on the generator
        if (rideGotDestroyed)
            GetComponentInChildren<Generator>().fightMusic.Play();
    }

    private void StartWave()
    {
        var _currentWave = waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()];

        foreach (var _enemyCluster in _currentWave.enemyClusters)
        {
            StartCoroutine(SpawnEnemyCluster(_enemyCluster));
        }
    }

    private IEnumerator SpawnEnemyCluster(EnemyClusterData enemyCluster)
    {
        yield return new WaitForSeconds(enemyCluster.timeToSpawn);
        SpawnEnemies(enemyCluster.enemyPrefab, enemyCluster.spawnCount);
    }
    
    private void LostWave()
    {
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicLoss");
        
        SetRide();
        rideGotDestroyed = true;
        
        foreach (var _enemy in currentEnemies)
        {
            Destroy(_enemy);
        }
    }

    private void WonWave()
    {
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicWon");
        GetComponent<Animator>().SetTrigger("StartRide");
        
        ActivationStatusInvisibleWalls(false);

        foreach (var _enemy in currentEnemies)
        {
            Destroy(_enemy);
        }
        
        GetComponentInChildren<Generator>().canPutAwayWalkieTalkie = true;
        currentTimeBetweenSpawns = maxTimeBetweenSpawns;
        waveStarted = false;
        Player.Instance.fightAreaCam.Priority = 5;
        InGameUIManager.Instance.fightScene.SetActive(false);
        InGameUIManager.Instance.radioAnim.SetTrigger("PutOn");
        InGameUIManager.Instance.ActivateRadio();
        GetComponentInChildren<Generator>().genInactive = true;
        GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount(GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished() + 1);
        GameSaveStateManager.Instance.SaveGame();
        AudioManager.Instance.Play("InGameMusic");
    }

    private void SpawnEnemies(GameObject enemyType, int enemyCount)
    {
        for (int _i = 0; _i < enemyCount; _i++)
        {
            currentEnemies.Add(Instantiate(enemyType, spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position, 
                Quaternion.identity, enemyParent.transform));
        }
    }
    
    private void SetRide()
    {
        waveTimer = 0;
        currentRideHealth = maxRideHealth;
        StartWave();
    }

    public void StartRideHitVisual()
    {
        if (rideGotHit)
            return;

        rideGotHit = true;

        SpriteRenderer _rideRenderer = GetComponent<SpriteRenderer>();
        _rideRenderer.color = Color.red;
        hitParticles.Play();

        Time.timeScale = 0.1f;
        
        StartCoroutine(StopRideHitVisual(_rideRenderer));
    }
    
    private IEnumerator StopRideHitVisual(SpriteRenderer rideRenderer)
    {
        yield return new WaitForSecondsRealtime(hitVisualTime);
        
        Time.timeScale = 1f;
        
        rideRenderer.color = Color.white;
        
        rideGotHit = false;
    }

    #endregion
}
