using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WeaponBehaviour : MonoBehaviour
{
    [Header("WEAPONS")]
    public List<WeaponObjectSO> allWeaponPrizes;
    
    [Header("Bullets")]
    [SerializeField] private ParticleSystem bulletShellsParticle;
    [HideInInspector] public float bulletDamage;
    [HideInInspector] public float bulletSpeed = 38f;
    [HideInInspector] public int maxPenetrationCount;
    [HideInInspector] public int bulletsPerShot;
    
    [Header("Ammo/Reload")]
    [HideInInspector] public int maxClipSize;
    [HideInInspector] public int ammunitionInClip;
    [HideInInspector] public int ammunitionInBackUp;
    public int ammunitionBackUpSize { private set; get; }
    private Coroutine currentReloadCoroutine;
    private float reloadTime;
    [SerializeField] private Image reloadProgress;
    public string noAmmoString = "NO AMMO LEFT";
    [SerializeField] private string ammoFullString = "AMMO FULL";

    [Header("Aiming")]
    public Transform weaponEndPoint;
    private Vector3 changingWeaponToMouse;
    private Vector3 weaponToMouse;
    [SerializeField] private int weaponRotationSnapPoints;
    [NonSerialized] public float LastSnappedAngle;
    
    [Header("Melee Weapon")]
    [SerializeField] private float getMeleeWeaponOutRange = 2f;
    private float currentGetMeleeWeaponOutRange;
    [SerializeField] private Sprite meleeWeapon;
    private bool meleeWeaponOut;
    [SerializeField] private float maxHitDelay;
    private float currentHitDelay;
    [Tooltip("The swing time has 3 phases - swinging back to get force, applying force, swinging back")]
    [SerializeField] private float[] swingTime;

    [Header("Shooting")]
    public float bulletDirectionSpreadStandingStill = 0.00005f;
    [FormerlySerializedAs("currentWeaponAccuracy")] [HideInInspector] public float currentBulletDirectionSpread;
    [FormerlySerializedAs("weaponAccuracy")] [HideInInspector] public float bulletDirectionSpread;
    [SerializeField] private float bulletSpawnSpread = 0.5f;    
    private float maxShootingDelay;
    private float currentShootingDelay;
    private bool isPressingLeftClick;
    
    [Header("Knock Back")]
    [HideInInspector] public float currentEnemyKnockBack;
    private float enemyShootingKnockBack;
    private float shootingKnockBack;
    [FormerlySerializedAs("CurrentKnockBack")] [HideInInspector] public Vector2 currentKnockBack;

    [Header("Camera")] 
    public CinemachineCamera playerCam;
    public Camera mainCamera;
    [Range(2, 10)] [SerializeField] private float cameraTargetLookAheadDivider;
    [SerializeField] private float fightCamOrthoSize = 9f;
    [SerializeField] private float normalCamOrthoSize = 6;
    [SerializeField] private float orthoSizeSmoothSpeed = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.2f;
    private Vector3 smoothedLookAhead;
    private Vector3 lookAheadVelocity;
    
    [Header("Weapon Visuals")]
    [SerializeField] private GameObject muzzleFlashVisual;
    [FormerlySerializedAs("longRangeWeapon")] [FormerlySerializedAs("weaponObject")] [SerializeField] private GameObject weapon;
    private Sprite longRangeWeaponSprite;
    private float weaponAimingAngle;
    private float weaponScreenShake;

    [HideInInspector] public string currentEquippedWeapon;
    
    [HideInInspector] public MyWeapon myWeapon;

    public enum MyWeapon
    {
        AssaultRifle,
        Shotgun,
        Magnum,
        PopcornLauncher,
        HuntingRifle,
    }

    private void Awake()
    {
        foreach (var _weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GetWeapon(_weapon);
        }
    }

    private void Start()
    {
        currentGetMeleeWeaponOutRange = getMeleeWeaponOutRange;
    }

    private void OnEnable()
    {
        GameInputManager.Instance.OnShootingAction += OnPressingShootingAction;
        GameInputManager.Instance.OnNotShootingAction += OnReleasingShootingAction;
        GameInputManager.Instance.OnReloadAction += OnPressingReloadingAction;
        GameInputManager.Instance.OnMeleeWeaponAction += OnPressingMeleeAction;
    }
    
    private void OnDisable()
    {
        GameInputManager.Instance.OnShootingAction -= OnPressingShootingAction;
        GameInputManager.Instance.OnNotShootingAction -= OnReleasingShootingAction;
        GameInputManager.Instance.OnReloadAction -= OnPressingReloadingAction;
        GameInputManager.Instance.OnMeleeWeaponAction -= OnPressingMeleeAction;
    }

    private void Update()
    {
        ShootAutomaticUpdate();

        HandleAimingUpdate();
        
        WeaponTimerUpdate();
        
        CamMovementUpdate();
    }

    #region Inputs

    private void OnReleasingShootingAction(object sender, EventArgs e)
    {
        isPressingLeftClick = false;
    }
    
    private void OnPressingShootingAction(object sender, EventArgs e)
    {
        if (weapon.activeSelf && !PlayerBehaviour.Instance.IsPlayerBusy() && !InGameUIManager.Instance.dialogueUI.IsDialoguePlaying())
        {
            isPressingLeftClick = true;
        }
        else
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueState();
        }
    }

    private void OnPressingReloadingAction(object sender, EventArgs e)
    {
        currentReloadCoroutine ??= StartCoroutine(ReloadCoroutine());
    }
    
    private void OnPressingMeleeAction(object sender, EventArgs eventArgs)
    {
        GetMeleeWeaponOut();
        
        HitAutomatic();
    }

    #endregion

    private void HandleAimingUpdate()
    {
        if (!weapon.activeSelf || TutorialManager.Instance.isExplainingCurrencyDialogue || (PlayerBehaviour.Instance.IsPlayerBusy() && !InGameUIManager.Instance.dialogueUI.IsDialoguePlaying())) 
            return;
        
        if (GetCurrentWeaponObjectSO() == null || GetComponent<MeleeWeaponBehaviour>().hitCollider.isActiveAndEnabled || (Mathf.Abs(weaponToMouse.x) <= currentGetMeleeWeaponOutRange && 
            Mathf.Abs(weaponToMouse.y) <= currentGetMeleeWeaponOutRange &&
            Mathf.Abs(weaponToMouse.x) >= 0 && 
            Mathf.Abs(weaponToMouse.y) >= 0)) 
        {
            GetMeleeWeaponOut();
        }
        else
        {
            currentEnemyKnockBack = enemyShootingKnockBack;
            
            weapon.GetComponent<SpriteRenderer>().sprite = longRangeWeaponSprite;
            
            weaponToMouse = GameInputManager.Instance.GetAimingVector() - weaponEndPoint.transform.position;
            weaponToMouse.z = 0;
            
            currentGetMeleeWeaponOutRange = getMeleeWeaponOutRange - getMeleeWeaponOutRange / 4;
            
            meleeWeaponOut = false;
        }
        
        weaponAimingAngle = Vector3.SignedAngle(Vector3.up, weaponToMouse, Vector3.forward);
        float _angle360 = weaponAimingAngle < 0 ? 360 + weaponAimingAngle : weaponAimingAngle;
        var _snapAngle = 360f / weaponRotationSnapPoints;
        
        //This removes jiggering because the player can now not aim in between two angles at the same time
        if (_angle360 < LastSnappedAngle - _snapAngle * .75f)
        {
            LastSnappedAngle = Mathf.Round(_angle360 / _snapAngle) * _snapAngle;
        }
        else if(_angle360 > LastSnappedAngle + _snapAngle * .75f)
        {
            LastSnappedAngle = Mathf.Round(_angle360 / _snapAngle) * _snapAngle;
        }

        transform.eulerAngles = new Vector3(0, 0, LastSnappedAngle);
    }

    public WeaponObjectSO GetCurrentWeaponObjectSO()
    {
        return PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == currentEquippedWeapon);
    }

    private void GetMeleeWeaponOut()
    {
        currentEnemyKnockBack = GetComponent<MeleeWeaponBehaviour>().knockBack;

        weapon.GetComponent<SpriteRenderer>().sprite = meleeWeapon;
            
        weaponToMouse = GameInputManager.Instance.GetAimingVector() - PlayerBehaviour.Instance.transform.position;
        weaponToMouse.z = 0;

        currentGetMeleeWeaponOutRange = getMeleeWeaponOutRange + getMeleeWeaponOutRange / 4;

        meleeWeaponOut = true;
    }

    public void CamMovementUpdate()
    {
        float _targetCamSize = normalCamOrthoSize;
        var _targetLookAhead = transform.parent.position;

        if (Ride.Instance.waveStarted)
        {
            _targetCamSize = fightCamOrthoSize;
            _targetLookAhead = (GameInputManager.Instance.GetAimingVector() + (cameraTargetLookAheadDivider - 1) * transform.position) / cameraTargetLookAheadDivider;
        }

        //check for vector zero because otherwise the cam would jump in positions when starting 
        if (smoothedLookAhead == Vector3.zero)
        {
            smoothedLookAhead = _targetLookAhead;
        }
        smoothedLookAhead = Vector3.SmoothDamp(smoothedLookAhead, _targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        // Reset the Z Pos because otherwise the cam is below the floor
        smoothedLookAhead.z = playerCam.transform.position.z;
        playerCam.transform.position = smoothedLookAhead;

        if(!Mathf.Approximately(playerCam.Lens.OrthographicSize, _targetCamSize))
        {
            playerCam.Lens.OrthographicSize = Mathf.Lerp(playerCam.Lens.OrthographicSize, _targetCamSize, Time.deltaTime * orthoSizeSmoothSpeed);
        }
    }
    
    private void ShootAutomaticUpdate()
    {
        if (!isPressingLeftClick) 
            return;
        
        if (meleeWeaponOut)
        {
            HitAutomatic();
            return;
        }
        
        if (ammunitionInClip <= 0)
        {
            currentReloadCoroutine ??= StartCoroutine(ReloadCoroutine());
            return;
        }
        
        if (currentReloadCoroutine != null)
        {
            AudioManager.Instance.Stop("Reload");
            StopCoroutine(currentReloadCoroutine);
            currentReloadCoroutine = null;
            reloadProgress.fillAmount = 0f;           
            PlayerBehaviour.Instance.currentMoveSpeed = PlayerBehaviour.Instance.baseMoveSpeed;
        }

        if (currentShootingDelay > 0)
        {
            return;
        }
        
        for (int _i = 0; _i < bulletsPerShot; _i++)
        {
            Vector2 _randomSpread = Random.insideUnitCircle * currentBulletDirectionSpread;            
            Vector2 _bulletDirection = ((Vector2)weaponToMouse.normalized + _randomSpread).normalized;

            Vector2 _randomSpawnOffset = Vector2.zero;
            if (bulletsPerShot > 1)
            {
                _randomSpawnOffset = Random.insideUnitCircle * bulletSpawnSpread;
            }
            Vector3 _spawnPosition = weaponEndPoint.position + new Vector3(_randomSpawnOffset.x, _randomSpawnOffset.y, 0f);
    
            var _bullet = BulletPoolingManager.Instance.GetInactiveBullet();
            _bullet.transform.SetPositionAndRotation(_spawnPosition, Quaternion.Euler(0, 0, weaponAimingAngle));
            _bullet.gameObject.SetActive(true);
            _bullet.LaunchInDirection(PlayerBehaviour.Instance, _bulletDirection);
        }

        currentShootingDelay = PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility == 
                               AbilityBehaviour.CurrentAbility.FastBullets ? 
                                PlayerBehaviour.Instance.abilityBehaviour.fastBulletsDelay : 
                                maxShootingDelay;

        StartCoroutine(WeaponVisualCoroutine());

        currentKnockBack = -weaponToMouse.normalized * shootingKnockBack;
            
        ammunitionInClip--;
        
        if (ammunitionInClip == 0 && ammunitionInBackUp == 0 && PlayerBehaviour.Instance.weaponBehaviour.GetCurrentWeaponObjectSO() != null)
        {
            PlayerBehaviour.Instance.ammoText.text = noAmmoString;
        }
                
        SetAmmunitionText(ammunitionInClip.ToString(), ammunitionInBackUp.ToString());
    }

    private void HitAutomatic()
    {
        if (currentHitDelay > 0 || PlayerBehaviour.Instance.IsPlayerBusy() || InGameUIManager.Instance.dialogueUI.IsDialoguePlaying())
            return;

        StartCoroutine(MeleeWeaponSwingCoroutine());
        
        currentHitDelay = maxHitDelay;
    }

    #region Ammo

    private IEnumerator ReloadCoroutine()
    {
        //If statement translation: no ammo overall or weapon already full or no weapon is equipped
        if (ammunitionInBackUp <= 0 || ammunitionInClip == maxClipSize || !weapon.activeSelf || PlayerBehaviour.Instance.gotHit || (PlayerBehaviour.Instance.IsPlayerBusy() && !InGameUIManager.Instance.dialogueUI.IsDialoguePlaying()))
        {
            //return and make some vfx
            yield break;
        }
        
        AudioManager.Instance.Play("Reload");
        reloadProgress.gameObject.SetActive(true);
        PlayerBehaviour.Instance.currentMoveSpeed = PlayerBehaviour.Instance.slowDownSpeed;

        float _elapsedTime = 0f;
        while (_elapsedTime < reloadTime)
        {
            _elapsedTime += Time.deltaTime;

            reloadProgress.fillAmount = Mathf.Clamp01(_elapsedTime / reloadTime);

            yield return null;
        }

        reloadProgress.gameObject.SetActive(false);
        reloadProgress.fillAmount = 0;
        PlayerBehaviour.Instance.currentMoveSpeed = PlayerBehaviour.Instance.baseMoveSpeed;

        //Calculates difference between the clipSize and how much is inside the clip (how much ammo we need for our reload)
        ammunitionInBackUp -= maxClipSize - ammunitionInClip;
        
        //If true, we do not have enough ammo for a full reload
        if (ammunitionInBackUp < 0)
        {
            //Calculates difference between the maxClipSize and how much we have for backup (how ammo can be inside the clip)
            var _ammoFromBackUpForClip = maxClipSize - Mathf.Abs(ammunitionInBackUp);
            ammunitionInClip = _ammoFromBackUpForClip;
            ammunitionInBackUp = 0;
            SetAmmunitionText(ammunitionInClip.ToString(), ammunitionInBackUp.ToString());
        }
        else
        {
            ammunitionInClip = maxClipSize;
            SetAmmunitionText(ammunitionInClip.ToString(), ammunitionInBackUp.ToString());
        }

        currentReloadCoroutine = null;
    }

    private void SetAmmunitionText(string clipAmmo, string backUpAmmo)
    {
        if(clipAmmo != null)
            InGameUIManager.Instance.ammunitionInClipText.text = clipAmmo;
        
        if(backUpAmmo != null)
            InGameUIManager.Instance.ammunitionInBackUpText.text = " " + backUpAmmo;
    }

    public void ObtainAmmoDrop(AmmoDrop ammoDrop, int setAmmoManually, bool fillClipAmmo)
    {
        if (ammoDrop == null)
        {
            ammunitionInBackUp = setAmmoManually;
        }
        else
        {
            if (ammunitionInBackUp < ammunitionBackUpSize)
            {
                ammunitionInBackUp += myWeapon switch
                {
                    MyWeapon.AssaultRifle => Mathf.RoundToInt(ammoDrop.ammoCount * 3.5f),
                    MyWeapon.Magnum => Mathf.RoundToInt(ammoDrop.ammoCount * 1.5f),
                    MyWeapon.PopcornLauncher => Mathf.RoundToInt(ammoDrop.ammoCount * 2.5f),
                    MyWeapon.HuntingRifle => ammoDrop.ammoCount,
                    MyWeapon.Shotgun => ammoDrop.ammoCount,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (ammunitionInBackUp > ammunitionBackUpSize)
                {
                    ammunitionInBackUp = ammunitionBackUpSize;
                }
                
                PlayerBehaviour.Instance.ammoText.text = "";

                Destroy(ammoDrop.gameObject);
            }
            else
            {
                PlayerBehaviour.Instance.ammoText.text = ammoFullString;
            }
        }

        if (fillClipAmmo)
        {
            ammunitionInClip = maxClipSize;
            SetAmmunitionText(ammunitionInClip.ToString(), ammunitionBackUpSize.ToString());
        }
        else
        {
            SetAmmunitionText(null, ammunitionInBackUp.ToString());
        }
    }

    #endregion

    private void WeaponTimerUpdate()
    {
        if (!weapon.activeSelf) 
            return;
        
        currentHitDelay -= Time.deltaTime;
        currentShootingDelay -= Time.deltaTime;
    }
    
    public void GetWeapon(WeaponObjectSO weapon)
    {
        if (GetCurrentWeaponObjectSO() != null)
        {
            GetCurrentWeaponObjectSO().ammunitionInClip = ammunitionInClip;
            GetCurrentWeaponObjectSO().ammunitionInBackUp = ammunitionInBackUp;   
        }

        this.weapon.SetActive(true);
        currentEquippedWeapon = weapon.weaponName;
        longRangeWeaponSprite = weapon.inGameWeaponVisual;
        bulletDamage = weapon.bulletDamage;
        maxPenetrationCount = weapon.penetrationCount;
        maxShootingDelay = weapon.shootDelay;
        bulletDirectionSpread = weapon.weaponBulletSpread;
        this.weapon.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.playerKnockBack;
        maxClipSize = weapon.clipSize;
        ammunitionInBackUp = weapon.ammunitionInBackUp;
        ammunitionBackUpSize = weapon.ammunitionBackUpSize;
        ammunitionInClip = weapon.ammunitionInClip;
        reloadTime = weapon.reloadTime;
        PlayerBehaviour.Instance.abilityBehaviour.hasAbilityUpgrade = weapon.hasAbilityUpgrade;
        SetAmmunitionText(weapon.ammunitionInClip.ToString(), weapon.ammunitionBackUpSize.ToString());
        AudioManager.Instance.ChangeSound("Shooting", weapon.shotSound);
        AudioManager.Instance.ChangeSound("Reload", weapon.reloadSound);
        AudioManager.Instance.ChangeSound("Repetition", weapon.repetitionSound);
        InGameUIManager.Instance.abilityProgressImage.color = weapon.abilityFillColor;
        foreach (var _bullet in BulletPoolingManager.Instance.GetBulletList())
        {
            _bullet.transform.localScale = weapon.bulletSize;
        }
        weaponScreenShake = weapon.screenShake;
        enemyShootingKnockBack = weapon.enemyKnockBackPerBullet;

        PlayerBehaviour.Instance.playerVisual.SetActive(false);
        PlayerBehaviour.Instance.playerNoHandVisual.SetActive(true);

        InGameUIManager.Instance.inGameUIWeaponVisual.GetComponent<Image>().sprite = weapon.uiWeaponVisual;
        InGameUIManager.Instance.inGameUIWeaponVisual.SetActive(true);
        
        myWeapon = weapon.weaponName switch
        {
            "Magnum magnum" => MyWeapon.Magnum,
            "French Fries AR" => MyWeapon.AssaultRifle,
            "Lollipop Shotgun" => MyWeapon.Shotgun,
            "Corn Dog Hunting Rifle" => MyWeapon.HuntingRifle,
            "Popcorn Launcher" => MyWeapon.PopcornLauncher,
            _ => myWeapon
        };
        
        GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(weapon.weaponName);
    }
    
    private IEnumerator WeaponVisualCoroutine()
    {
        muzzleFlashVisual.SetActive(true);
        
        playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = weaponScreenShake;

        bulletShellsParticle.Play();

        AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        bulletShellsParticle.Stop();

        playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = 0;

        muzzleFlashVisual.SetActive(false);
        
        AudioManager.Instance.Play("Repetition");
    }
    
    IEnumerator MeleeWeaponSwingCoroutine()
    {
        var _steps = new[]
        {
            (-50f, Random.Range(swingTime[0] - .01f, swingTime[0] + .01f)),
            (230f, Random.Range(swingTime[1] - .01f, swingTime[1] + .01f)),
            (-170f, Random.Range(swingTime[2] - .01f, swingTime[2] + .01f)),
        };

        TrailRenderer _trailRendererBaton = GetComponentInChildren<TrailRenderer>();

        for (var _index = 0; _index < _steps.Length; _index++)
        {
            if (_index == 1)
            {
                _trailRendererBaton.emitting = true;
                GetComponent<MeleeWeaponBehaviour>().hitCollider.enabled = true;
            }
            
            (float _deltaZ, float _duration) = _steps[_index];
            float _elapsed = 0f;
            
            float _startZ = transform.localRotation.eulerAngles.z;
            float _targetZ = _startZ + _deltaZ;
            
            float _startZBaton = _trailRendererBaton.transform.localRotation.eulerAngles.z;
            float _swingTargetZBaton = _startZBaton - 90;
            float _pullBackZBaton = _startZBaton + 90;

            while (_elapsed < _duration)
            {
                _elapsed += Time.deltaTime;
                
                float _currentZ = Mathf.Lerp(_startZ, _targetZ, _elapsed / _duration);
                transform.localRotation = Quaternion.Euler(0f, 0f, _currentZ);
                
                switch (_index)
                {
                    case 0:
                    {
                        float _currentZBat = Mathf.Lerp(_startZBaton, _swingTargetZBaton, _elapsed / _duration);
                        _trailRendererBaton.transform.localRotation = Quaternion.Euler(0f, 0f, _currentZBat);
                        break;
                    }
                    case 1:
                    {
                        AudioManager.Instance.Play("BatonSwing");
                        break;
                    }
                    case 2:
                    {
                        float _currentZBat = Mathf.Lerp(_startZBaton, _pullBackZBaton, _elapsed / _duration);
                        _trailRendererBaton.transform.localRotation = Quaternion.Euler(0f, 0f, _currentZBat);
                        break;
                    }
                }

                yield return null;
            }
            
            transform.localRotation = Quaternion.Euler(0f, 0f, _targetZ);
        }
        
        _trailRendererBaton.Clear();
        _trailRendererBaton.emitting = false;
        
        GetComponent<MeleeWeaponBehaviour>().hitCollider.enabled = false;

        meleeWeaponOut = false;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(weaponEndPoint.position, bulletSpawnSpread);
    }
}
