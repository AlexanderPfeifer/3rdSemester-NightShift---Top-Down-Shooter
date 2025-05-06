using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ride : Singleton<Ride>
{
    [Header("Spawning")]
    [SerializeField] private Wave[] waves;
    [HideInInspector] public bool waveStarted;
    private float waveTimer;
    public GameObject enemyParent;
    public List<GameObject> currentEnemies = new();
    
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
    public SpriteRenderer rideRenderer;
    private Animator rideAnimator;

    [Header("Activation")]
    public GameObject rideLight;
    [HideInInspector] public Generator generator;
    
    [Header("Arena")]
    public GameObject invisibleCollider;

    private void Start()
    {
        InGameUIManager.Instance.dialogueUI.dialogueCountShop = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
        rideAnimator = GetComponentInChildren<Animator>();
        generator = GetComponentInChildren<Generator>();
    }

    private void Update()
    {
        TimerUpdate();
    }
    
    private void TimerUpdate()
    {
        if (!waveStarted)
        {
            waveTimer = GetCurrentWave().maxWaveTime;
            return;
        }
        
        InGameUIManager.Instance.rideHpImage.fillAmount = currentRideHealth / maxRideHealth;
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

    public void LostWave()
    {
        generator.fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicLoss");
        
        ResetRide();
        
        generator.interactable = true;
        
        StopAllCoroutines();
        rideAnimator.SetTrigger("LightOff");
        rideLight.SetActive(false);

        waveStarted = false;

        for (int _i = 0; _i < enemyParent.transform.childCount; _i++)
        {
            Transform _child = enemyParent.transform.GetChild(_i);

            if (_child.TryGetComponent(out EnemyBase _enemyBase))
            {
                _enemyBase.addHelpDropsOnDeath = false;
                _enemyBase.destroyWithoutEffect = true;
            }
            
            Destroy(_child.gameObject);
        }
    }

    private void WonWave()
    {
        TutorialManager.Instance.newWeaponsCanBeUnlocked = true;
        
        generator.fightMusic.Stop();
        AudioManager.Instance.Play("FightMusicWon");

        invisibleCollider.SetActive(false);
        
        StopAllCoroutines();
        rideAnimator.SetTrigger("LightOff");
        rideLight.SetActive(false);
        
        List<GameObject> _toDestroy = new List<GameObject>();

        foreach (var _enemy in currentEnemies)
        {
            if (_enemy.TryGetComponent(out EnemyBase _enemyBase))
            {
                _enemyBase.addHelpDropsOnDeath = false;
            }

            _toDestroy.Add(_enemy);
        }

        foreach (var _enemy in _toDestroy)
        {
            Destroy(_enemy);
        }
        
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(
            Mathf.RoundToInt(GetCurrentWave().currencyPrize * (currentRideHealth / maxRideHealth)), true);
        
        ResetRide();

        generator.canPutAwayWalkieTalkie = true;
        waveStarted = false;
        InGameUIManager.Instance.fightUI.SetActive(false);
        InGameUIManager.Instance.abilityFillBar.SetActive(false);
        InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
        generator.interactable = true;
        GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount();

        if (GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished() == waves.Length)
        {
            rideAnimator.SetTrigger("StartRide");
        }
        
        GameSaveStateManager.Instance.SaveGame();
        AudioManager.Instance.FadeIn("InGameMusic");
    }

    #region Ride

    public void ResetRide()
    {
        waveTimer = 0;
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
