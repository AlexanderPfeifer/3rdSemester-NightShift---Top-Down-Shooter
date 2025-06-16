using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Ride : Singleton<Ride>
{
    [Header("Spawning")] 
    [SerializeField] private Wave[] waves;
    [HideInInspector] public bool waveStarted;
    public GameObject enemyParent;
    [SerializeField] private float radiusInsideGroupSpawning = 2;
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private float squareSizeY;
    [SerializeField] private float squareSizeX;

    [Header("Health")] 
    public Image rideHealthFill;
    public float maxRideHealth;
    [HideInInspector] public float currentRideHealth;

    [Header("HitVisual")]
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private ParticleSystem hitParticles;
    private bool rideGotHit;
    public SpriteRenderer rideRenderer;

    [Header("Activation")]
    public GameObject[] rideLight;
    public RideActivation rideActivation;

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

    public void StartEnemyClusterCoroutines()
    {
        if (DebugMode.Instance != null)
        {
            DebugMode.Instance.AddWaves();
        }
        
        foreach (var _enemyCluster in waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()].enemyClusters)
        {            
            spawnedEnemiesInCluster += _enemyCluster.enemyPrefab.Length * _enemyCluster.spawnCount * _enemyCluster.repeatCount;
            
            StartCoroutine(SpawnEnemies(_enemyCluster));
        }
    }

    private IEnumerator SpawnEnemies(EnemyClusterData enemyCluster)
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

    public void LostWave()
    {
        AudioManager.Instance.Play("FightMusicLoss");
        
        ResetRide();
        StartCoroutine(LooseVisuals());
    }

    private IEnumerator LooseVisuals()
    {
        PlayerBehaviour.Instance.SetPlayerBusy(true);
        var shutter = InGameUIManager.Instance.shutterLooseImage.rectTransform;

        while (AudioManager.Instance.IsPlaying("FightMusicLoss"))
        {
            hitParticles.Play();
            yield return new WaitForSeconds(0.75f);
        }

        AudioManager.Instance.Play("MetalShutterDown");

        yield return MoveShutter(shutter, shutter.localPosition.y, 0f, shutterGoDownTime);

        PlayerBehaviour.Instance.transform.position = restartPosition;
        CleanStageFromEnemies();

        yield return new WaitForSeconds(1);

        //Set player busy false because otherwise the shop won't open 
        PlayerBehaviour.Instance.SetPlayerBusy(false);
        InGameUIManager.Instance.OpenShop();

        float targetY = 1100f;
        yield return MoveShutter(shutter, shutter.localPosition.y, targetY, 0f);

        InGameUIManager.Instance.dialogueUI.StopCurrentAndTypeNewTextCoroutine(peggyLooseText, null,InGameUIManager.Instance.dialogueUI.currentTextBox);
    }

    private IEnumerator MoveShutter(RectTransform shutter, float startY, float endY, float duration)
    {
        float elapsed = 0f;
        Vector3 pos = shutter.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY, endY, elapsed / duration);
            shutter.localPosition = new Vector3(pos.x, newY, pos.z);
            yield return null;
        }

        shutter.localPosition = new Vector3(pos.x, endY, pos.z);
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
            Mathf.RoundToInt(waves[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished()].currencyPrize * (currentRideHealth / maxRideHealth)), true);
        
        ResetRide();
        
        AudioManager.Instance.Play("FightMusicWon");

        var dialogueUI = InGameUIManager.Instance.dialogueUI;

        GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount();

        if (TutorialManager.Instance.explainedRideSequences)
        {
            StartCoroutine(PlayRideSoundsAfterOneAnother());

            if (dialogueUI.dialogueCountWalkieTalkie < dialogueUI.dialogueWalkieTalkie.Length)
            {
                dialogueUI.SetDialogueBoxState(true, true);
            }
        }
        else
        {
            dialogueUI.SetDialogueBoxState(true, true);
        }
        
        AudioManager.Instance.FadeIn("InGameMusic");

        GameSaveStateManager.Instance.SaveGame();
    }
    
    private IEnumerator PlayRideSoundsAfterOneAnother()
    {
        PlayerBehaviour.Instance.SetPlayerBusy(true);

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
            fuses[GameSaveStateManager.Instance.saveGameDataManager.HasWavesFinished() - 2].sprite = (i % 2 == 0) ? DeactivateFuse() : ActivateFuse();
            yield return new WaitForSeconds(0.6f);
        }

        yield return new WaitForSeconds(.5f);

        InGameUIManager.Instance.generatorUI.gameObject.SetActive(false);

        rideActivation.gateAnim.SetBool("OpenGate", true);

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

    private void CleanStageFromEnemies()
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

    public void ResetRide()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        rideRenderer.color = Color.white;
        rideGotHit = false;
        waveStarted = false;
        rideActivation.fightMusic.Stop();
        rideActivation.interactable = true;
    }

    public void DealDamage(float rideAttackDamage)
    {
        currentRideHealth -= rideAttackDamage;
        rideHealthFill.fillAmount = currentRideHealth / maxRideHealth;

        AudioManager.Instance.Play("RideHit");

        StartRideHitVisual();

        if (currentRideHealth <= 0)
        {
            LostWave();
        }
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