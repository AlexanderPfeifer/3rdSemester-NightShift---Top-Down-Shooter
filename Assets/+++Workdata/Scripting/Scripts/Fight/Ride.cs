using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ride : Singleton<Ride>
{
    [Header("Spawning")] [SerializeField] private Wave[] waves;
    [HideInInspector] public bool waveStarted;
    private float waveTimer;
    public GameObject enemyParent;

    [Header("Square Spawn Point")] [SerializeField]
    private Transform spawnCenter;

    [SerializeField] private float squareSizeY;
    [SerializeField] private float squareSizeX;

    [Header("Health")] 
    public float maxRideHealth;  
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private ParticleSystem hitParticles;
    [HideInInspector] public float currentRideHealth;
    private bool rideGotHit;
    public SpriteRenderer rideRenderer;
    private Animator rideAnimator;

    [Header("Activation")]
    public GameObject rideLight;
    public Generator generator;

    private void Start()
    {
        InGameUIManager.Instance.dialogueUI.dialogueCountShop = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
        rideAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        TimerUpdate();
    }
    
    private void TimerUpdate()
    {
        if (!waveStarted)
        {
            return;
        }
        
        TimeSpan _timeSpan = TimeSpan.FromSeconds(waveTimer);
        InGameUIManager.Instance.rideTimeText.text = _timeSpan.ToString(@"mm\:ss");        
        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0)
        {
            WonWave();
        }
    }

    private Wave GetCurrentWave()
    {
        return waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()];
    }
    
    public int GetCurrentWaveAsInt()
    {
        return GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
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
                Instantiate(enemyCluster.enemyPrefab, GetRandomEdgePosition(), Quaternion.identity, enemyParent.transform);
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

    public void LostWave()
    {
        AudioManager.Instance.Play("FightMusicLoss");
        
        RemoveEnemiesFromStage(true);
        
        SetFightState();
    }

    private void WonWave()
    {
        InGameUIManager.Instance.SetWalkieTalkieQuestLog(TutorialManager.Instance.getNewWeapons);
        
        AudioManager.Instance.Play("FightMusicWon");
        RemoveEnemiesFromStage(false);

        PlayerBehaviour.Instance.playerCurrency.AddCurrency(
            Mathf.RoundToInt(GetCurrentWave().currencyPrize * (currentRideHealth / maxRideHealth)), true);
        
        SetFightState();

        InGameUIManager.Instance.fightUI.SetActive(false);
        
        if(InGameUIManager.Instance.dialogueUI.dialogueCountWalkieTalkie < InGameUIManager.Instance.dialogueUI.dialogueWalkieTalkie.Length)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
        }
        
        GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount();

        if (GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished() == waves.Length)
        {
            rideLight.SetActive(true);
            rideAnimator.SetTrigger("StartRide");
        }
        
        GameSaveStateManager.Instance.SaveGame();
    }

    private void SetFightState()
    {
        waveStarted = false;
        rideLight.SetActive(false);
        StopAllCoroutines();
        ResetRide();
        generator.fightMusic.Stop();
        generator.interactable = true;
        AudioManager.Instance.FadeIn("InGameMusic");
    }

    private void RemoveEnemiesFromStage(bool destroyEnemyWithoutEffect)
    {
        for (int _i = 0; _i < enemyParent.transform.childCount; _i++)
        {
            Transform _child = enemyParent.transform.GetChild(_i);

            if (_child.TryGetComponent(out EnemyBase _enemyBase))
            {
                _enemyBase.addHelpDropsOnDeath = false;
                _enemyBase.destroyWithoutEffect = destroyEnemyWithoutEffect;

                Destroy(_child.gameObject);
            }
        }
    }

    #region Ride

    public void ResetRide()
    {
        generator.gateAnim.SetBool("OpenGate", true);
        waveTimer = GetCurrentWave().maxWaveTime;
        currentRideHealth = maxRideHealth;
        
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
