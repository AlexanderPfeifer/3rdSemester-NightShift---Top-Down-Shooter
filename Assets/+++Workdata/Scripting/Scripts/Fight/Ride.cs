using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ride : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private Wave[] waves;
    [HideInInspector] public bool waveStarted;
    private float waveTimer;
    public GameObject enemyParent;
    private readonly List<GameObject> currentEnemies = new();
    
    [Header("Square Spawn Point")]
    [SerializeField] private Transform spawnCenter; 
    [SerializeField] private float squareSizeY; 
    [SerializeField] private float squareSizeX; 

    [Header("Health")]
    [SerializeField] private float maxRideHealth = 50;
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private ParticleSystem hitParticles;
    [HideInInspector] public float currentRideHealth;
    private bool rideGotHit;
    [HideInInspector] public bool rideGotDestroyed;
    [SerializeField] private SpriteRenderer rideRenderer;

    [Header("Activation")]
    public GameObject rideLight;
    
    [Header("Arena")]
    public GameObject invisibleCollider;

    private void Start()
    {
        InGameUIManager.Instance.dialogueUI.dialogueCount = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
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
        InGameUIManager.Instance.rideTimeImage.fillAmount = waveTimer / GetCurrentWave().maxWaveTime;
        waveTimer += Time.deltaTime;

        if (waveTimer >= GetCurrentWave().maxWaveTime)
        {
            WonWave();
        }
        else if (currentRideHealth <= 0)
        {
            LostWave();
        }
    }

    private Wave GetCurrentWave()
    {
        return waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()];
    }

    #region EnemySpawning

    public void StartEnemyClusterCoroutines()
    {
        foreach (var _enemyCluster in GetCurrentWave().enemyClusters)
        {
            StartCoroutine(SpawnEnemiesDelayed(_enemyCluster));
        }
    }

    private IEnumerator SpawnEnemiesDelayed(EnemyClusterData enemyCluster)
    {
        yield return new WaitForSeconds(enemyCluster.spawnStartTime);

        for (int _i = 0; _i < enemyCluster.repeatCount; _i++)
        {
            for (int _enemyIndex = 0; _enemyIndex < enemyCluster.spawnCount; _enemyIndex++)
            {
                currentEnemies.Add(Instantiate(enemyCluster.enemyPrefab,
                    GetRandomEdgePosition(), Quaternion.identity, enemyParent.transform));
            }
            
            yield return new WaitForSeconds(enemyCluster.timeBetweenSpawns);
        }
    }
    
    private Vector3 GetRandomEdgePosition()
    {
        float _halfSizeY = squareSizeY / 2f;
        float _halfSizeX = squareSizeX / 2f;
        Vector3 _spawnPos = spawnCenter.position;

        switch (Random.Range(0, 4))
        {
            case 0: // Top Edge
                _spawnPos += new Vector3(Random.Range(-_halfSizeY, _halfSizeY), _halfSizeY, 0);
                break;
            case 1: // Right Edge
                _spawnPos += new Vector3(_halfSizeX, Random.Range(-_halfSizeX, _halfSizeX), 0);
                break;
            case 2: // Bottom Edge
                _spawnPos += new Vector3(Random.Range(-_halfSizeY, _halfSizeY), -_halfSizeY, 0);
                break;
            case 3: // Left Edge
                _spawnPos += new Vector3(-_halfSizeX, Random.Range(-_halfSizeX, _halfSizeX), 0);
                break;
        }
        return _spawnPos;
    }

    #endregion

    private void LostWave()
    {
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicLoss");
        
        ResetRide();
        rideGotDestroyed = true;

        GetComponentInChildren<Generator>().genInteractable = true;
        
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
        TutorialManager.Instance.openShutterWheelOfFortune = true;
        
        GetComponentInChildren<Generator>().fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicWon");

        invisibleCollider.SetActive(false);
        
        StopAllCoroutines();
        GetComponent<Animator>().SetTrigger("LightOff");
        rideLight.SetActive(false);
        
        foreach (var _enemy in currentEnemies)
        {
            Destroy(_enemy);
        }
        
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(
            Mathf.RoundToInt(GetCurrentWave().currencyPrize * (currentRideHealth / maxRideHealth)));
        
        ResetRide();

        GetComponentInChildren<Generator>().canPutAwayWalkieTalkie = true;
        waveStarted = false;
        PlayerBehaviour.Instance.weaponBehaviour.fightAreaCam.Priority = 5;
        InGameUIManager.Instance.fightUI.SetActive(false);
        InGameUIManager.Instance.abilityFillBar.SetActive(false);
        InGameUIManager.Instance.dialogueUI.SetRadioState(true, true);
        GetComponentInChildren<Generator>().genInteractable = true;
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
        
        Time.timeScale = 1f;
        rideRenderer.color = Color.white;
        rideGotHit = false;
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
    
    private void OnDrawGizmos()
    {
        if (spawnCenter == null) 
            return;

        Vector3 _center = spawnCenter.position;
        float _halfSizeY = squareSizeY / 2;
        float _halfSizeX = squareSizeX / 2;

        Vector3 _topLeft = _center + new Vector3(-_halfSizeX, _halfSizeY, 0);
        Vector3 _topRight = _center + new Vector3(_halfSizeX, _halfSizeY, 0);
        Vector3 _bottomRight = _center + new Vector3(_halfSizeX, -_halfSizeY, 0);
        Vector3 _bottomLeft = _center + new Vector3(-_halfSizeX, -_halfSizeY, -0);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_topLeft, _topRight);
        Gizmos.DrawLine(_topRight, _bottomRight);
        Gizmos.DrawLine(_bottomRight, _bottomLeft);
        Gizmos.DrawLine(_bottomLeft, _topLeft);
    }
}
