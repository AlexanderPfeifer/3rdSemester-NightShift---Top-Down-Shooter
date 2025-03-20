using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private int ammunitionInBackUp;
    private Coroutine currentReloadCoroutine;
    [SerializeField] private float reloadTime = 2;
    [SerializeField] private Image reloadProgress;

    [Header("Aiming")]
    public Transform weaponEndPoint;
    private Vector3 changingWeaponToMouse;
    private Vector3 weaponToMouse;
    private Vector3 mousePos;
    
    [Header("Shooting")]
    private float maxShootingDelay;
    private float currentShootingDelay;
    private float weaponAccuracy;
    private bool isShooting;
    
    [Header("Knock Back")]
    public float enemyShootingKnockBack = 2;
    private float shootingKnockBack;
    [NonSerialized] public Vector2 CurrentKnockBack;
    
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    public CinemachineCamera fightAreaCam;
    [Range(2, 10)] [SerializeField] private float cameraTargetLookAheadDivider;
    [SerializeField] private float fightCamOrthoSize = 8f;
    [SerializeField] private float orthoSizeSmoothSpeed = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.2f;
    private Vector3 smoothedLookAhead;
    private Vector3 lookAheadVelocity;
    
    [Header("Visuals")]
    [SerializeField] private GameObject muzzleFlashVisual;
    private Animator weaponAnim;
    [SerializeField] private float weaponToMouseSmoothness = 8;
    private float weaponAngleSmoothed;
    private float weaponAngleUnSmoothed;
    private float weaponScreenShake;
    
    [HideInInspector] public MyWeapon myWeapon;

    public enum MyWeapon
    {
        AssaultRifle,
        Shotgun,
        Magnum,
        Pistol,
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
        weaponAnim = GetComponentInChildren<Animator>();
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
    }

    private void OnReleasingShootingAction(object sender, EventArgs e)
    {
        isShooting = false;
    }
    
    private void OnPressingShootingAction(object sender, EventArgs e)
    {
        if (!InGameUIManager.Instance.inventoryIsOpened && !InGameUIManager.Instance.shopScreen.activeSelf && 
            weaponAnim.gameObject.activeSelf && !PlayerBehaviour.Instance.isInteracting && 
            InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueNotPlaying)
        {
            isShooting = true;
        }
        else
        {
            switch (InGameUIManager.Instance.dialogueState)
            {
                case InGameUIManager.DialogueState.DialoguePlaying:
                    InGameUIManager.Instance.textDisplaySpeed = InGameUIManager.Instance.maxTextDisplaySpeed;
                    break;
                case InGameUIManager.DialogueState.DialogueAbleToGoNext:
                    InGameUIManager.Instance.PlayNextDialogue();
                    break;
                case InGameUIManager.DialogueState.DialogueAbleToEnd:
                    InGameUIManager.Instance.EndDialogue();
                    break;
                case InGameUIManager.DialogueState.DialogueNotPlaying:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnPressingReloadingAction(object sender, EventArgs e)
    {
        currentReloadCoroutine ??= StartCoroutine(ReloadCoroutine());
    }
    
    private void HandleAimingUpdate()
    {
        if (InGameUIManager.Instance.inventoryIsOpened || !weaponAnim.gameObject.activeSelf || InGameUIManager.Instance.shopScreen.activeSelf) 
            return;
        
        mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        //Do cam lookahead before we zero out the mousePos.z so we do not divide by 0
        if (fightAreaCam.Priority > 10)
        {
            if(!Mathf.Approximately(fightAreaCam.Lens.OrthographicSize, fightCamOrthoSize))
            {
                fightAreaCam.Lens.OrthographicSize = Mathf.Lerp(fightAreaCam.Lens.OrthographicSize, fightCamOrthoSize, Time.deltaTime * orthoSizeSmoothSpeed);
            }

            var _fightCamTransform = fightAreaCam.transform;
            var _fightCamZPos = _fightCamTransform.position.z;
            var _targetLookAhead = (mousePos + (cameraTargetLookAheadDivider - 1) * transform.position) / cameraTargetLookAheadDivider;
            
            //check for vector zero because otherwise the cam would jump in positions when starting 
            if (smoothedLookAhead == Vector3.zero)
            {
                smoothedLookAhead = _targetLookAhead;
            }
            smoothedLookAhead = Vector3.SmoothDamp(smoothedLookAhead, _targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

            // Reset the Z Pos because otherwise the cam is below the floor
            smoothedLookAhead.z = _fightCamZPos;
            _fightCamTransform.position = smoothedLookAhead;
        }
        
        mousePos.z = 0;

        /*
         Check for the distance of mouse to the character and 
         increase the actual mouse position so it does not start glitching.
         */
        if (Vector3.Distance(transform.position, mousePos) >= 1.35f)
        {
            if (Vector3.Distance(weaponEndPoint.position, mousePos) <= 2f)
            {
                changingWeaponToMouse = mousePos - weaponEndPoint.transform.position;
                
                if (weaponToMouse.x < 0)
                {
                    weaponToMouse = (changingWeaponToMouse * -2).normalized;
                }
                
                if(weaponToMouse.x > 0)
                {
                    weaponToMouse = (changingWeaponToMouse * 2).normalized;
                }
            }
            else
            {
                weaponToMouse = mousePos - weaponEndPoint.transform.position;
            }
        }

        Vector3 _newUp = Vector3.Slerp(transform.up, weaponToMouse, Time.deltaTime * weaponToMouseSmoothness);

        weaponAngleSmoothed = Vector3.SignedAngle(Vector3.up, _newUp, Vector3.forward);
        weaponAngleUnSmoothed = Vector3.SignedAngle(Vector3.up, weaponToMouse, Vector3.forward);
        
        transform.eulerAngles = new Vector3(0, 0, weaponAngleSmoothed);
        
        if (transform.eulerAngles.z is > 0 and < 180)
        {
            transform.GetComponentInChildren<SpriteRenderer>().sortingOrder = - 1;
        }
        else
        {
            transform.GetComponentInChildren<SpriteRenderer>().sortingOrder = + 1;
        }
    }
    
    private void ShootAutomaticUpdate()
    {
        if (!isShooting || currentShootingDelay > 0) 
            return;
        
        if (ammunitionInClip > 0)
        {
            if (currentReloadCoroutine != null)
            {
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
                _bullet.transform.SetPositionAndRotation(_spawnPosition, Quaternion.Euler(0, 0, weaponAngleUnSmoothed));
                _bullet.gameObject.SetActive(true);
                _bullet.LaunchInDirection(PlayerBehaviour.Instance, _bulletDirection);
            }

            currentShootingDelay = PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility == AbilityBehaviour.CurrentAbility.FastBullets ? PlayerBehaviour.Instance.abilityBehaviour.fastBulletsDelay : maxShootingDelay;

            StartCoroutine(WeaponVisualCoroutine());

            CurrentKnockBack = -weaponToMouse.normalized * shootingKnockBack;
            
            ammunitionInClip--;
                
            SetAmmunitionText(ammunitionInClip.ToString(), ammunitionInBackUp.ToString());
        }
        else
        {
            currentReloadCoroutine ??= StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        //If statement translation: no ammo overall or weapon already full or no weapon is equipped
        if (ammunitionInBackUp <= 0 || ammunitionInClip == maxClipSize || !weaponAnim.gameObject.activeSelf)
        {
            //return and make some vfx
            yield break;
        }
        
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
    
    private void WeaponTimerUpdate()
    {
        if (weaponAnim.gameObject.activeSelf)
        {
            currentShootingDelay -= Time.deltaTime;
        }
    }
    
    public void GetWeapon(WeaponObjectSO weapon)
    {
        weaponAnim.gameObject.SetActive(true);
        weaponAnim.gameObject.GetComponent<SpriteRenderer>().sprite = weapon.inGameWeaponVisual;
        bulletDamage = weapon.bulletDamage;
        maxPenetrationCount = weapon.penetrationCount;
        maxShootingDelay = weapon.shootDelay;
        weaponAccuracy = weapon.weaponSpread;
        weaponAnim.gameObject.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.playerKnockBack;
        maxClipSize = weapon.clipSize;
        ammunitionInBackUp = weapon.ammunitionInBackUp;
        ammunitionInClip = weapon.ammunitionInClip;
        PlayerBehaviour.Instance.abilityBehaviour.hasAbilityUpgrade = weapon.hasAbilityUpgrade;
        SetAmmunitionText(weapon.ammunitionInClip.ToString(), weapon.ammunitionInBackUp.ToString());
        foreach (var _bullet in BulletPoolingManager.Instance.GetBulletList())
        {
            _bullet.transform.localScale = weapon.bulletSize;
        }
        weaponScreenShake = weapon.screenShake;
        enemyShootingKnockBack = weapon.enemyKnockBack;

        PlayerBehaviour.Instance.playerVisual.SetActive(false);
        PlayerBehaviour.Instance.playerNoHandVisual.SetActive(true);

        InGameUIManager.Instance.equippedWeapon.GetComponent<Image>().sprite = weapon.inGameWeaponVisual;
        InGameUIManager.Instance.equippedWeapon.SetActive(true);
        
        myWeapon = weapon.weaponName switch
        {
            
            "Magnum magnum" => MyWeapon.Magnum,
            "French Fries AR" => MyWeapon.AssaultRifle,
            "Lollipop Shotgun" => MyWeapon.Shotgun,
            "Corn Dog Hunting Rifle" => MyWeapon.HuntingRifle,
            "Popcorn Launcher" => MyWeapon.Pistol,
            _ => myWeapon
        };
    }
    
    private IEnumerator WeaponVisualCoroutine()
    {
        muzzleFlashVisual.SetActive(true);
        
        if (fightAreaCam.Priority > 10)
        {
            fightAreaCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = weaponScreenShake;
        }
        
        bulletShellsParticle.Play();

        weaponAnim.SetTrigger("ShootGun");
        
        AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        weaponAnim.SetTrigger("GunStartPos");

        bulletShellsParticle.Stop();

        if (fightAreaCam.Priority > 10)
        {
            fightAreaCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = 0;
        }

        muzzleFlashVisual.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out AmmoDrop _ammoDrop))
        {
            ammunitionInBackUp += myWeapon switch
            {
                MyWeapon.AssaultRifle => _ammoDrop.ammoCount * 5,
                MyWeapon.Magnum => _ammoDrop.ammoCount * 2,
                MyWeapon.Pistol => _ammoDrop.ammoCount * 3,
                MyWeapon.HuntingRifle => Mathf.RoundToInt(_ammoDrop.ammoCount * 1.5f),
                MyWeapon.Shotgun => _ammoDrop.ammoCount,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            SetAmmunitionText(null, ammunitionInBackUp.ToString());
            
            Destroy(other.gameObject);
        }
    }
}
