using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ride : MonoBehaviour
{
    [Header("Spawning")]
    public Wave[] waves;
    [HideInInspector] public bool waveStarted;
    private float waveTimer;
    [SerializeField] private List<SpawnPoint> spawnPoints;
    [SerializeField] private GameObject enemyParent;
    private readonly List<GameObject> currentEnemies = new();
    
    [Header("Health")]
    [SerializeField] private float maxRideHealth = 50;
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private ParticleSystem hitParticles;
    [HideInInspector] public float currentRideHealth;
    private bool rideGotHit;
    [HideInInspector] public bool rideGotDestroyed;
    private SpriteRenderer rideRenderer;

    [Header("Activation")]
    public GameObject rideLight;
    
    [Header("Arena")]
    public GameObject invisibleCollider;

    private void Start()
    {
        InGameUIManager.Instance.dialogueCount = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
        rideRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        TimerUpdate();
    }
    
    private void TimerUpdate()
    {
        if (!waveStarted) 
            return;
        
        InGameUIManager.Instance.rideHpImage.fillAmount = currentRideHealth / maxRideHealth;
        InGameUIManager.Instance.rideTimeImage.fillAmount = waveTimer / waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()].maxWaveTime;
        waveTimer += Time.deltaTime;

        if (waveTimer >= waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()].maxWaveTime)
        {
            WonWave();
        }
        else if (currentRideHealth <= 0)
        {
            LostWave();
        }
    }

    #region EnemySpawning

    public void StartEnemyClusterCoroutines()
    {
        foreach (var _enemyCluster in waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()].enemyClusters)
        {
            StartCoroutine(SpawnEnemiesDelayed(_enemyCluster));
        }
    }

    private IEnumerator SpawnEnemiesDelayed(EnemyClusterData enemyCluster)
    {
        yield return new WaitForSeconds(enemyCluster.timeToSpawn);

        for (int _i = 0; _i < enemyCluster.repeatCount; _i++)
        {
            for (int _enemyIndex = 0; _enemyIndex < enemyCluster.spawnCount; _enemyIndex++)
            {
                currentEnemies.Add(Instantiate(enemyCluster.enemyPrefab,
                    spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position, Quaternion.identity, enemyParent.transform));
            }
            
            yield return new WaitForSeconds(enemyCluster.timeBetweenSpawns);
        }
    }

    #endregion

    private void LostWave()
    {
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicLoss");
        
        ResetRide();
        rideGotDestroyed = true;
        
        Time.timeScale = 1f;
        rideRenderer.color = Color.white;
        rideGotHit = false;
        
        StopAllCoroutines();
        
        GetComponent<Animator>().SetTrigger("LightOff");
        rideLight.SetActive(false);
        
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
        
        invisibleCollider.SetActive(false);
        
        foreach (var _enemy in currentEnemies)
        {
            Destroy(_enemy);
        }
        
        GetComponentInChildren<Generator>().canPutAwayWalkieTalkie = true;
        waveStarted = false;
        Player.Instance.fightAreaCam.Priority = 5;
        InGameUIManager.Instance.fightScene.SetActive(false);
        InGameUIManager.Instance.radioAnim.SetTrigger("PutOn");
        InGameUIManager.Instance.ActivateRadio();
        GetComponentInChildren<Generator>().genInactive = true;
        GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount();
        GameSaveStateManager.Instance.SaveGame();
        AudioManager.Instance.Play("InGameMusic");
    }

    #region Ride

    public void ResetRide()
    {
        waveTimer = 0;
        currentRideHealth = maxRideHealth;
        InGameUIManager.Instance.abilityProgressImage.fillAmount = 0;
    }

    public void StartRideHitVisual()
    {
        if (rideGotHit)
            return;

        rideGotHit = true;

        rideRenderer.color = Color.red;
        hitParticles.Play();

        Time.timeScale = 0.1f;
        
        StartCoroutine(StopRideHitVisual());
    }
    
    private IEnumerator StopRideHitVisual()
    {
        yield return new WaitForSecondsRealtime(hitVisualTime);
        
        Time.timeScale = 1f;
        rideRenderer.color = Color.white;
        rideGotHit = false;
    }

    #endregion
    
    private void OnValidate()
    {
        for (var _i = 0; _i < waves.Length; _i++)
        {
            waves[_i].waveName = "Wave " + _i;

            foreach (var _cluster in waves[_i].enemyClusters)
            {
                _cluster.UpdateClusterName();
            }
        }
    }
}
