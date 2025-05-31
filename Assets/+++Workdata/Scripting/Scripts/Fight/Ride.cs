using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ride : Singleton<Ride>
{
    [Header("Spawning")] [SerializeField] private Wave[] waves;
    [HideInInspector] public bool waveStarted;
    public GameObject enemyParent;
    [SerializeField] private float radiusInsideGroupSpawning = 2;
    
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

    [Header("Win")] 
    [SerializeField] private ParticleSystem winConfettiParticles;
    private float latestSpawnedEnemy;
    private float timeUntilLastEnemy;
    [HideInInspector] public bool canWinGame;

    private void Start()
    {
        InGameUIManager.Instance.dialogueUI.dialogueCountShop = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
        rideAnimator = GetComponentInChildren<Animator>();
    }

    private Wave GetCurrentWave()
    {
        return waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()];
    }
    
    public int GetCurrentWaveAsInt()
    {
        return GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
    }

    private void Update()
    {
        if (waveStarted)
        {
            timeUntilLastEnemy += Time.deltaTime;
        }
    }

    #region EnemySpawning

    public void StartEnemyClusterCoroutines()
    {
        latestSpawnedEnemy = 0;
        timeUntilLastEnemy = 0;

        foreach (var _enemyCluster in GetCurrentWave().enemyClusters)
        {
            StartCoroutine(SpawnEnemiesDelayed(_enemyCluster));

            float _timeWhenEnemyStopsSpawning = _enemyCluster.spawnStartTime + _enemyCluster.repeatCount * _enemyCluster.timeBetweenSpawns;

            if (latestSpawnedEnemy < _timeWhenEnemyStopsSpawning)
            {
                latestSpawnedEnemy = _timeWhenEnemyStopsSpawning;
            }
        }
    }

    private IEnumerator SpawnEnemiesDelayed(EnemyClusterData enemyCluster)
    {
        yield return new WaitForSeconds(enemyCluster.spawnStartTime);

        for (int _i = 0; _i < enemyCluster.repeatCount; _i++)
        {
            for (int _enemyIndex = 0; _enemyIndex < enemyCluster.spawnCount; _enemyIndex++)
            {
                Vector2 _groupPos = GetRandomEdgePosition();
                
                foreach (var _enemy in enemyCluster.enemyPrefab)
                {
                    Vector2 _randomOffset = Random.insideUnitCircle * radiusInsideGroupSpawning;
                    Instantiate(_enemy, _groupPos + new Vector2(_randomOffset.x, _randomOffset.y), Quaternion.identity, enemyParent.transform);
                }
            }
            
            if(enemyCluster.repeatCount != _i - 1)
                yield return new WaitForSeconds(enemyCluster.timeBetweenSpawns);
        }
        
        //I add a buffer of -1 if the timeUntilLastEnemy is a bit too late for some reason
        if (timeUntilLastEnemy >= latestSpawnedEnemy - 1)
        {
            canWinGame = true;
        }
    }
    
    private Vector2 GetRandomEdgePosition()
    {
        float _halfSizeY = squareSizeY / 2f;
        float _halfSizeX = squareSizeX / 2f;
        Vector2 _spawnPos = spawnCenter.position;

        switch (Random.Range(0, 4))
        {
            case 0: // Top Edge
                _spawnPos += new Vector2(Random.Range(-_halfSizeY, _halfSizeY), _halfSizeY);
                break;
            case 1: // Right Edge
                _spawnPos += new Vector2(_halfSizeX, Random.Range(-_halfSizeX, _halfSizeX));
                break;
            case 2: // Bottom Edge
                _spawnPos += new Vector2(Random.Range(-_halfSizeY, _halfSizeY), -_halfSizeY);
                break;
            case 3: // Left Edge
                _spawnPos += new Vector2(-_halfSizeX, Random.Range(-_halfSizeX, _halfSizeX));
                break;
        }
        
        return _spawnPos;
    }

    #endregion

    public void LostWave()
    {
        AudioManager.Instance.Play("FightMusicLoss");
        
        RemoveEnemiesFromStage(true);
        
        rideLight.SetActive(false);

        SetFightState();
    }

    public void WonWave()
    {
        winConfettiParticles.Play();
            
        InGameUIManager.Instance.SetWalkieTalkieQuestLog(TutorialManager.Instance.getNewWeapons);
        
        RemoveEnemiesFromStage(false);

        PlayerBehaviour.Instance.playerCurrency.AddCurrency(
            Mathf.RoundToInt(GetCurrentWave().currencyPrize * (currentRideHealth / maxRideHealth)), true);
        
        SetFightState();
        
        AudioManager.Instance.Play("FightMusicWon");

        if (TutorialManager.Instance.explainedRideSequences)
        {
            StartCoroutine(PlayRideSoundsAfterOneAnother());
        }
        
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
    
    private IEnumerator PlayRideSoundsAfterOneAnother()
    {
        while (AudioManager.Instance.IsPlaying("FightMusicWon"))
        {
            yield return null;
        }
        
        AudioManager.Instance.Play("RideShutDown");
        rideLight.SetActive(false);

        yield return null;
    }

    private void SetFightState()
    {
        waveStarted = false;
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
        if(TutorialManager.Instance.explainedRideSequences)
            generator.gateAnim.SetBool("OpenGate", true);
        
        currentRideHealth = maxRideHealth;
        InGameUIManager.Instance.rideHpImage.fillAmount = currentRideHealth / maxRideHealth;
        canWinGame = false;
        
        Time.timeScale = 1f;
        rideRenderer.color = Color.white;
        rideGotHit = false;
    }

    public void StartRideHitVisual()
    {
        hitParticles.Play();
        
        if (rideGotHit)
            return;
        
        rideGotHit = true;
        
        rideRenderer.color = Color.red;
        
        Time.timeScale = 0.1f;
        
        StartCoroutine(HitStop());
    }
    
    private IEnumerator HitStop()
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
