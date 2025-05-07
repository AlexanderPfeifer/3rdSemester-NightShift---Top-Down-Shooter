using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor.Animations;
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
    private int bulletsPerShot;
    [Range(.1f, .3f), SerializeField] private float bulletSpread = .2f;
    
    [Header("Ammo/Reload")]
    private int maxClipSize;
    private int ammunitionInClip;
    public int ammunitionInBackUp { private set; get; }
    private Coroutine currentReloadCoroutine;
    private float reloadTime;
    [SerializeField] private Image reloadProgress;

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
    [SerializeField] private AnimatorController meleeWeaponAnimatorController;

    [Header("Shooting")]
    private float maxShootingDelay;
    private float currentShootingDelay;
    private float weaponAccuracy;
    private bool isPressingLeftClick;
    
    [Header("Knock Back")]
    public float enemyShootingKnockBack = 2;
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
    private Animator anim;
    [SerializeField] private AnimatorController weaponAnimatorController;
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
        anim = weapon.GetComponent<Animator>();
        currentGetMeleeWeaponOutRange = getMeleeWeaponOutRange;
    }

    private void OnEnable()
    {
        GameInputManager.Instance.OnShootingAction += OnPressingShootingAction;
        GameInputManager.Instance.OnNotShootingAction += OnReleasingShootingAction;
        GameInputManager.Instance.OnReloadAction += OnPressingReloadingAction;
    }
    
    private void OnDisable()
    {
        GameInputManager.Instance.OnShootingAction -= OnPressingShootingAction;
        GameInputManager.Instance.OnNotShootingAction -= OnReleasingShootingAction;
        GameInputManager.Instance.OnReloadAction -= OnPressingReloadingAction;
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

    #endregion

    private void HandleAimingUpdate()
    {
        if (!weapon.activeSelf || PlayerBehaviour.Instance.IsPlayerBusy()) 
            return;

        var _shortGetMeleeWeaponOutRange = getMeleeWeaponOutRange - getMeleeWeaponOutRange / 2;
        var _longGetMeleeWeaponOutRange = getMeleeWeaponOutRange + getMeleeWeaponOutRange / 2;
        
        if (Mathf.Abs(weaponToMouse.x) <= currentGetMeleeWeaponOutRange && 
            Mathf.Abs(weaponToMouse.y) <= currentGetMeleeWeaponOutRange &&
            Mathf.Abs(weaponToMouse.x) >= 0 && 
            Mathf.Abs(weaponToMouse.y) >= 0) 
        {
            weapon.GetComponent<SpriteRenderer>().sprite = meleeWeapon;
            
            anim.runtimeAnimatorController = meleeWeaponAnimatorController;

            weaponToMouse = GameInputManager.Instance.GetAimingVector() - PlayerBehaviour.Instance.transform.position;
            weaponToMouse.z = 0;

            currentGetMeleeWeaponOutRange = _longGetMeleeWeaponOutRange;

            meleeWeaponOut = true;
        }
        else
        {
            weapon.GetComponent<SpriteRenderer>().sprite = longRangeWeaponSprite;

            anim.runtimeAnimatorController = weaponAnimatorController;
            
            weaponToMouse = GameInputManager.Instance.GetAimingVector() - weaponEndPoint.transform.position;
            weaponToMouse.z = 0;
            
            currentGetMeleeWeaponOutRange = _shortGetMeleeWeaponOutRange;
            
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

    public void CamMovementUpdate()
    {
        float _targetCamSize = normalCamOrthoSize;
        var _targetLookAhead = transform.parent.position;

        if (Ride.Instance.waveStarted)
        {
            _targetCamSize = fightCamOrthoSize;
            _targetLookAhead = ((Vector3)GameInputManager.Instance.GetAimingVector() + (cameraTargetLookAheadDivider - 1) * transform.position) / cameraTargetLookAheadDivider;
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
        if (!isPressingLeftClick || currentShootingDelay > 0) 
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

        for (int _i = 0; _i < bulletsPerShot; _i++)
        {
            Vector2 _bulletDirection = Random.insideUnitCircle.normalized;
            _bulletDirection = Vector3.Slerp(_bulletDirection, weaponToMouse.normalized, 1.0f - weaponAccuracy);

            Vector2 _perpendicularOffset = new Vector2(-_bulletDirection.y, _bulletDirection.x);
            // This calculation is a perfect spread of bullets(ask ChatGPT) 
            float _spreadOffset = (_i - (bulletsPerShot - 1) / 2f) * bulletSpread;
            Vector3 _spawnPosition = weaponEndPoint.position + (Vector3)(_perpendicularOffset * _spreadOffset);
    
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
                
        SetAmmunitionText(ammunitionInClip.ToString(), ammunitionInBackUp.ToString());
    }

    private void HitAutomatic()
    {
        if (currentHitDelay > 0) 
            return;

        anim.SetTrigger("Hit");
        
        currentHitDelay = maxHitDelay;
    }

    #region Ammo

    private IEnumerator ReloadCoroutine()
    {
        //If statement translation: no ammo overall or weapon already full or no weapon is equipped
        if (ammunitionInBackUp <= 0 || ammunitionInClip == maxClipSize || !weapon.activeSelf)
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
            InGameUIManager.Instance.ammunitionInBackUpText.text = "/" + backUpAmmo;
    }

    public void ObtainAmmoDrop(AmmoDrop ammoDrop, int setAmmoManually, bool fillClipAmmo)
    {
        if (ammoDrop == null)
        {
            ammunitionInBackUp = setAmmoManually;
        }
        else
        {
            ammunitionInBackUp += myWeapon switch
            {
                MyWeapon.AssaultRifle => ammoDrop.ammoCount * 5,
                MyWeapon.Magnum => ammoDrop.ammoCount * 2,
                MyWeapon.PopcornLauncher => ammoDrop.ammoCount * 3,
                MyWeapon.HuntingRifle => Mathf.RoundToInt(ammoDrop.ammoCount * 1.5f),
                MyWeapon.Shotgun => ammoDrop.ammoCount,
                _ => throw new ArgumentOutOfRangeException()
            };
        
            Destroy(ammoDrop.gameObject);
        }

        if (fillClipAmmo)
        {
            ammunitionInClip = maxClipSize;
            SetAmmunitionText(ammunitionInClip.ToString(), ammunitionInBackUp.ToString());
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
        
        if (!meleeWeaponOut)
        {
            currentShootingDelay -= Time.deltaTime;   
        }
        else
        {
            currentHitDelay -= Time.deltaTime;
        }
    }
    
    public void GetWeapon(WeaponObjectSO weapon)
    {
        this.weapon.SetActive(true);
        currentEquippedWeapon = weapon.weaponName;
        longRangeWeaponSprite = weapon.inGameWeaponVisual;
        bulletDamage = weapon.bulletDamage;
        maxPenetrationCount = weapon.penetrationCount;
        maxShootingDelay = weapon.shootDelay;
        weaponAccuracy = weapon.weaponSpread;
        this.weapon.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.playerKnockBack;
        maxClipSize = weapon.clipSize;
        ammunitionInBackUp = weapon.ammunitionInBackUp;
        ammunitionInClip = weapon.ammunitionInClip;
        reloadTime = weapon.reloadTime;
        PlayerBehaviour.Instance.abilityBehaviour.hasAbilityUpgrade = weapon.hasAbilityUpgrade;
        SetAmmunitionText(weapon.ammunitionInClip.ToString(), weapon.ammunitionInBackUp.ToString());
        AudioManager.Instance.ChangeSound("Shooting", weapon.shotSound);
        AudioManager.Instance.ChangeSound("Reload", weapon.reloadSound);
        AudioManager.Instance.ChangeSound("Repetition", weapon.repetitionSound);
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

        anim.SetTrigger("ShootGun");
        
        AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        anim.SetTrigger("GunStartPos");

        bulletShellsParticle.Stop();

        playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = 0;

        muzzleFlashVisual.SetActive(false);
        
        AudioManager.Instance.Play("Repetition");
    }
}
