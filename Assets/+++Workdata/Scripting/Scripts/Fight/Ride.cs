using Newtonsoft.Json.Bson;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    public Image rideHealthFill;
    public float maxRideHealth;  
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private ParticleSystem hitParticles;
    [HideInInspector] public float currentRideHealth;
    private bool rideGotHit;
    public SpriteRenderer rideRenderer;

    [Header("Activation")]
    public GameObject[] rideLight;
    public Generator generator;

    [Header("Win")] 
    [SerializeField] private ParticleSystem winConfettiParticles;
    private int currentSpawnedEnemies;
    private int spawnedEnemiesInCluster;
    [HideInInspector] public bool canWinGame;
    public Image[] fuses;
    public Sprite activeFuse;
    public Sprite inActiveFuse;

    [Header("Loose")]
    [SerializeField] private Vector2 restartPosition;
    [SerializeField] private float shutterGoDownTime;
    [SerializeField, TextArea(3, 10)] private string peggyLooseText;

    private void Start()
    {
        InGameUIManager.Instance.dialogueUI.dialogueCountShop = GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished();
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
        if (DebugMode.Instance != null)
        {
            DebugMode.Instance.AddWaves();
        }
        
        foreach (var _enemyCluster in GetCurrentWave().enemyClusters)
        {            
            spawnedEnemiesInCluster += _enemyCluster.enemyPrefab.Length * _enemyCluster.spawnCount * _enemyCluster.repeatCount;
            
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
                Vector2 _groupPos = GetRandomEdgePosition();
                
                foreach (var _enemy in enemyCluster.enemyPrefab)
                {
                    Vector2 _randomOffset = Random.insideUnitCircle * radiusInsideGroupSpawning;
                    Instantiate(_enemy, _groupPos + new Vector2(_randomOffset.x, _randomOffset.y), Quaternion.identity, enemyParent.transform);
                    currentSpawnedEnemies++;
                }
            }
            
            if(_i != enemyCluster.repeatCount - 1)
                yield return new WaitForSeconds(enemyCluster.timeBetweenSpawns);
        }
        
        if (spawnedEnemiesInCluster == currentSpawnedEnemies)
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
        
        CleanUpRide();
        StartCoroutine(LooseVisuals());
    }

    public void WonWave()
    {
        winConfettiParticles.Play();

        foreach (var _balloonCart in FindObjectsByType<BalloonCartBehaviour>(FindObjectsSortMode.None))
        {
            _balloonCart.ResetBalloons();
        }
            
        InGameUIManager.Instance.SetWalkieTalkieQuestLog(TutorialManager.Instance.getNewWeapons);
        
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(
            Mathf.RoundToInt(GetCurrentWave().currencyPrize * (currentRideHealth / maxRideHealth)), true);
        
        CleanUpRide();
        
        AudioManager.Instance.Play("FightMusicWon");

        if (TutorialManager.Instance.explainedRideSequences && waves.Length != GetCurrentWaveAsInt() + 2)
        {
            StartCoroutine(PlayRideSoundsAfterOneAnother());

            if (InGameUIManager.Instance.dialogueUI.dialogueCountWalkieTalkie < InGameUIManager.Instance.dialogueUI.dialogueWalkieTalkie.Length)
            {
                InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
            }
        }

        if (!TutorialManager.Instance.explainedRideSequences)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
        }

        GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount();

        if(TutorialManager.Instance.explainedRideSequences)
            generator.gateAnim.SetBool("OpenGate", true);
        
        AudioManager.Instance.FadeIn("InGameMusic");

        GameSaveStateManager.Instance.SaveGame();
    }
    
    private IEnumerator PlayRideSoundsAfterOneAnother()
    {
        while (AudioManager.Instance.IsPlaying("FightMusicWon"))
        {
            yield return null;
        }
        
        AudioManager.Instance.Play("RideShutDown");
        foreach (var _light in rideLight)
        {
            _light.SetActive(false);
        }

        while (AudioManager.Instance.IsPlaying("RideShutDown"))
        {
            yield return null;
        }

        InGameUIManager.Instance.generatorUI.gameObject.SetActive(true);

        yield return new WaitForSeconds (.5f);

        for (int i = 0; i < 6; i++)
        {
            fuses[GetCurrentWaveAsInt() - 1].sprite = (i % 2 == 0) ? DeactivateFuse() : ActivateFuse();
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(.5f);

        InGameUIManager.Instance.generatorUI.gameObject.SetActive(false);

        yield return null;
    }

    public Sprite DeactivateFuse()
    {
        AudioManager.Instance.Play("FuseOff");

        return inActiveFuse;
    }

    public Sprite ActivateFuse()
    {
        AudioManager.Instance.Play("FuseOn");

        return activeFuse;
    }

    private void CleanUpStage()
    {
        for (int _i = 0; _i < enemyParent.transform.childCount; _i++)
        {
            Transform _child = enemyParent.transform.GetChild(_i);

            if (_child.TryGetComponent(out EnemyBase _enemyBase))
            {
                _enemyBase.addHelpDropsOnDeath = false;
            }
            
            Destroy(_child.gameObject);
        }
    }

    #region Ride

    private IEnumerator LooseVisuals()
    {
        PlayerBehaviour.Instance.SetPlayerBusy(true);
        
        while (AudioManager.Instance.IsPlaying("FightMusicLoss"))
        {
            hitParticles.Play();
            yield return new WaitForSeconds(0.75f);
        }
        
        AudioManager.Instance.Play("MetalShutterDown");
        
        float _elapsedTime = 0f;
        var _rectTransform = InGameUIManager.Instance.shutterLooseImage.rectTransform.localPosition;
        float _startFill = _rectTransform.y;
        float _targetPosition = 0f;

        while (_elapsedTime < shutterGoDownTime)
        {
            _elapsedTime += Time.deltaTime;
            InGameUIManager.Instance.shutterLooseImage.rectTransform.localPosition = 
                new Vector3(_rectTransform.x, Mathf.Lerp(_startFill, _targetPosition, _elapsedTime / shutterGoDownTime), _rectTransform.z);
            yield return null;
        }

        InGameUIManager.Instance.shutterLooseImage.rectTransform.localPosition = 
            new Vector3(_rectTransform.x, _targetPosition, _rectTransform.z);
        PlayerBehaviour.Instance.transform.position = restartPosition;
        CleanUpStage();

        yield return new WaitForSeconds(1);
        
        PlayerBehaviour.Instance.SetPlayerBusy(false);
        InGameUIManager.Instance.OpenShop();

        _elapsedTime = 0f;
        _startFill = InGameUIManager.Instance.shutterLooseImage.rectTransform.localPosition.y;
        _targetPosition = 1100f;
        shutterGoDownTime = 0f;

        while (_elapsedTime < shutterGoDownTime)
        {
            _elapsedTime += Time.deltaTime;
            InGameUIManager.Instance.shutterLooseImage.rectTransform.localPosition = 
                new Vector3(_rectTransform.x, Mathf.Lerp(_startFill, _targetPosition, _elapsedTime / shutterGoDownTime), _rectTransform.z);            
            yield return null;
        }
        
        InGameUIManager.Instance.dialogueUI.StopCurrentAndTypeNewTextCoroutine(peggyLooseText, null, InGameUIManager.Instance.dialogueUI.currentTextBox);

        InGameUIManager.Instance.shutterLooseImage.rectTransform.localPosition = new Vector3(_rectTransform.x, _targetPosition, _rectTransform.z);
    }

    private void CleanUpRide()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        rideRenderer.color = Color.white;
        rideGotHit = false;
        waveStarted = false;
        generator.fightMusic.Stop();
        generator.interactable = true;
    }

    public void ResetRide()
    {
        currentRideHealth = maxRideHealth;
        rideHealthFill.fillAmount = currentRideHealth / maxRideHealth;
        canWinGame = false;
        CleanUpRide();
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
