using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, SavableVector3> PositionBySceneName = new();
    }
    
    public static Player Instance;

    [Header("Scripts")]
    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] private PlayerSaveData playerSaveData;

    [Header("CharacterMovement")] 
    [SerializeField] private float baseMoveSpeed = 6.25f;
    [SerializeField] private float slowDownSpeed;
    [SerializeField] private float knockBackDecay = 5f; 
    private float currentMoveSpeed;
    private Vector2 moveDirection = Vector2.down;
    private Vector2 knockBack;
    private Rigidbody2D rb;
    [SerializeField] private GameObject playerVisual;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject playerNoHandVisual;
    [SerializeField] private Animator animNoHand;
    
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [FormerlySerializedAs("fightAreaCams")] public CinemachineCamera fightAreaCam;
    [Range(2, 10)] [SerializeField] private float cameraTargetLookAheadDivider;
    [SerializeField] private float fightCamOrthoSize = 8f;
    [SerializeField] private float orthoSizeSmoothSpeed = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.2f;
    private Vector3 smoothedLookAhead;
    private Vector3 lookAheadVelocity;

    [Header("Light")] 
    [SerializeField] public GameObject globalLightObject;

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

    [Header("Weapon")]
    public List<WeaponObjectSO> allWeaponPrizes;
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    public float enemyShootingKnockBack = 2;
    private float maxShootingDelay;
    private float currentShootingDelay;
    private float weaponAccuracy;
    private Vector3 changingWeaponToMouse;
    private Vector3 weaponToMouse;
    private Vector3 mousePos;
    private float shootingKnockBack;
    private bool isShooting;
    private bool hasWeapon;
    
    [Header("Weapon Ability")]
    [HideInInspector] public bool freezeBullets;
    [HideInInspector] public bool stickyBullets;
    [HideInInspector] public bool explosiveBullets;
    [HideInInspector] public bool endlessPenetrationBullets;
    [HideInInspector] public bool fastBullets;
    [SerializeField] public float maxAbilityTime;
    [SerializeField] private float fastBulletsDelay = 0.075f;
    [HideInInspector] public float currentAbilityTime;
    [HideInInspector] public bool canGetAbilityGain = true;
    private bool isUsingAbility;
    
    [Header("WeaponVisuals")]
    public GameObject weaponVisual;
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] private Animator weaponAnim;
    [SerializeField] private float weaponToMouseSmoothness = 8;
    private float weaponAngleSmoothed;
    private float weaponAngleUnSmoothed;
    private float weaponScreenShake;
    
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2;
    [SerializeField] public LayerMask wheelOfFortuneLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] public LayerMask rideLayer;
    [SerializeField] public LayerMask duckLayer;
    [SerializeField] private LayerMask collectibleLayer;
    [HideInInspector] public bool canInteract = true;
    [HideInInspector] public bool isInteracting;
    [HideInInspector] public int enemyWave;

    private MyWeapon myWeapon;
    enum MyWeapon
    {
        AssaultRifle,
        Shotgun,
        Magnum,
        Pistol,
        HuntingRifle,
    }
    
    private void SetupFromData()
    {
        if (playerSaveData.PositionBySceneName.TryGetValue(gameObject.scene.name, out var _position))
            transform.position = _position;
    }

    #region MonoBehaviourMethods

    private void Awake()
    {
        Instance = this;
        
        foreach (var _weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GetWeapon(_weapon);
        }
        
        var _currentPlayerSaveData = GameSaveStateManager.Instance.saveGameDataManager.newPlayerSaveData;
        if (_currentPlayerSaveData != null)
        {
            playerSaveData = _currentPlayerSaveData;
            SetupFromData();
        }
        
        GameSaveStateManager.Instance.saveGameDataManager.newPlayerSaveData = playerSaveData;
    }

    private void OnEnable()
    {
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnNotShootingAction += GameInputManagerOnNotShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction += GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction += GameInputManagerOnUsingAbilityAction;
        gameInputManager.OnSprintingAction += GameInputManagerOnSprintingAction;
        gameInputManager.OnReloadAction += GameInputManagerOnReloadingAction;
        AudioManager.Instance.Play("InGameMusic");
    }

    private void OnDisable()
    {
        gameInputManager.OnShootingAction -= GameInputManagerOnShootingAction;
        gameInputManager.OnNotShootingAction -= GameInputManagerOnNotShootingAction;
        gameInputManager.OnGamePausedAction -= GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction -= GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction -= GameInputManagerOnUsingAbilityAction;
        gameInputManager.OnSprintingAction -= GameInputManagerOnSprintingAction;
    }

    private void Start()
    {
        InGameUIManager.Instance.loadingScreenAnim.SetTrigger("End");
        InGameUIManager.Instance.ActivateInGameUI();
        rb = GetComponent<Rigidbody2D>();
        currentMoveSpeed = baseMoveSpeed;

        if (DebugMode.Instance.debugMode)
        {
            FindAnyObjectByType<Generator>().GetComponent<Generator>().SetUpFightArena();
            
            transform.position = new Vector3(38, 4, 0);
            
            DebugMode.Instance.GetDebugWeapon();
        }
    }

    private void Update()
    { 
        HandleAimingUpdate();

        WeaponTimerUpdate();
        
        ShootAutomaticUpdate();

        HandleInteractionIndicator();
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }
    
    private void LateUpdate()
    {
        /*
        we have to save the current position dependant on the scene the player is in.
        this way, the position can be retained across multiple scenes, and we can switch back and forth.
        */
        playerSaveData.PositionBySceneName[gameObject.scene.name] = transform.position;
        
        SetAnimationParameterLateUpdate();
    }

    #endregion

    #region Input

    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if (!InGameUIManager.Instance.inventoryIsOpened && !isUsingAbility && weaponVisual.activeSelf && !isInteracting && InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueNotPlaying)
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

    private void GameInputManagerOnNotShootingAction(object sender, EventArgs e)
    {
        isShooting = false;
    }

    private void GameInputManagerOnSprintingAction(object sender, EventArgs e)
    {
        
    }

    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        if(InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueNotPlaying)
            InGameUIManager.Instance.OpenInventory();
    }

    private void GameInputManagerOnInteractAction(object sender, EventArgs e)
    {
        if (GetInteractionObjectInRange(collectibleLayer, out Collider2D _collectible))
        {
            _collectible.GetComponent<Collectible>().Collect();
        }
        else if (GetInteractionObjectInRange(wheelOfFortuneLayer, out _) && !InGameUIManager.Instance.fortuneWheelScreen.activeSelf)
        {
            foreach (var _weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
            {
                InGameUIManager.Instance.weaponSwapScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(InGameUIManager.Instance.firstSelectedWeaponDecision);
                InGameUIManager.Instance.weaponDecisionWeaponAbilityText.text = "";
                InGameUIManager.Instance.weaponDecisionWeaponName.text = "";
                InGameUIManager.Instance.weaponDecisionWeaponImage.GetComponent<Image>().sprite = _weapon.inGameWeaponVisual;
                InGameUIManager.Instance.weaponDecisionWeaponAbilityText.text += "Special Ability:" + "\n" + _weapon.weaponAbilityDescription;
                InGameUIManager.Instance.weaponDecisionWeaponName.text += _weapon.weaponName;
                isInteracting = true;
                hasWeapon = true;
            }

            if (!hasWeapon)
            {
                InGameUIManager.Instance.fortuneWheelScreen.SetActive(true);
            }
        }
        else if (GetInteractionObjectInRange(generatorLayer, out Collider2D _generator) && !InGameUIManager.Instance.generatorScreen.activeSelf)
        {
            if (_generator.GetComponent<Generator>().isInteractable)
            {
                InGameUIManager.Instance.generatorScreen.SetActive(true);
                GameSaveStateManager.Instance.SaveGame();
            }
        }
        else if (GetInteractionObjectInRange(duckLayer, out _))
        {
            AudioManager.Instance.Play("DuckSound");
        }
        else if (GetInteractionObjectInRange(rideLayer, out Collider2D _interactable))
        {
            var _ride = _interactable.gameObject.GetComponent<Ride>();
            
            if (_ride.canActivateRide)
            {
                _ride.SetWave();
            }
        }
    }

    private void GameInputManagerOnUsingAbilityAction(object sender, EventArgs e)
    {
        if (currentAbilityTime >= maxAbilityTime && InGameUIManager.Instance.fightScene.activeSelf)
        {
            StartCoroutine(StartWeaponAbility());
        }
    }
    
    private void GameInputManagerOnReloadingAction(object sender, EventArgs e)
    {
        currentReloadCoroutine ??= StartCoroutine(ReloadCoroutine());
    }

    #endregion

    #region Shooting

    private void HandleAimingUpdate()
    {
        if (InGameUIManager.Instance.inventoryIsOpened || !weaponVisual.activeSelf) 
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

        Vector3 _newUp = Vector3.Slerp(weaponPosParent.transform.up, weaponToMouse, Time.deltaTime * weaponToMouseSmoothness);

        weaponAngleSmoothed = Vector3.SignedAngle(Vector3.up, _newUp, Vector3.forward);
        weaponAngleUnSmoothed = Vector3.SignedAngle(Vector3.up, weaponToMouse, Vector3.forward);
        
        weaponPosParent.eulerAngles = new Vector3(0, 0, weaponAngleSmoothed);
        
        if (weaponPosParent.eulerAngles.z is > 0 and < 180)
        {
            weaponPosParent.GetComponentInChildren<SpriteRenderer>().sortingOrder = - 1;
        }
        else
        {
            weaponPosParent.GetComponentInChildren<SpriteRenderer>().sortingOrder = + 1;
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
                currentMoveSpeed = baseMoveSpeed;
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
                _bullet.LaunchInDirection(this, _bulletDirection);
            }
                
            currentShootingDelay = fastBullets ? fastBulletsDelay : maxShootingDelay;

            StartCoroutine(WeaponVisualCoroutine());

            knockBack = -weaponToMouse.normalized * shootingKnockBack;
            
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
        if (ammunitionInBackUp <= 0 || ammunitionInClip == maxClipSize || !weaponVisual.activeSelf)
        {
            //return and make some vfx
            yield break;
        }
        
        reloadProgress.gameObject.SetActive(true);
        currentMoveSpeed = slowDownSpeed;

        float _elapsedTime = 0f;
        while (_elapsedTime < reloadTime)
        {
            _elapsedTime += Time.deltaTime;

            reloadProgress.fillAmount = Mathf.Clamp01(_elapsedTime / reloadTime);

            yield return null;
        }

        reloadProgress.gameObject.SetActive(false);
        reloadProgress.fillAmount = 0;
        currentMoveSpeed = baseMoveSpeed;

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

    #endregion

    #region Movement

    private void HandleMovementFixedUpdate()
    {
        if (InGameUIManager.Instance.dialogueState != InGameUIManager.DialogueState.DialogueNotPlaying || InGameUIManager.Instance.inventoryIsOpened || isInteracting) 
            return;
        
        rb.linearVelocity = gameInputManager.GetMovementVectorNormalized() * currentMoveSpeed + knockBack;

        knockBack = Vector2.Lerp(knockBack, Vector2.zero, Time.fixedDeltaTime * knockBackDecay);
    }
    
    private void SetAnimationParameterLateUpdate()
    {
        if (!isInteracting && InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueNotPlaying)
        {
            if (playerVisual.activeSelf)
            {
                anim.SetFloat("MoveSpeed", rb.linearVelocity.sqrMagnitude);

                if (gameInputManager.GetMovementVectorNormalized().sqrMagnitude <= 0)
                {
                    return;
                }
        
                moveDirection = gameInputManager.GetMovementVectorNormalized();
                anim.SetFloat("MoveDirX", moveDirection.x);
                anim.SetFloat("MoveDirY", moveDirection.y);
            }
            else
            {
                var _eulerAngles = weaponPosParent.eulerAngles;
                animNoHand.SetBool("MovingUp", _eulerAngles.z is < 45 or > 315);
                animNoHand.SetBool("MovingDown", _eulerAngles.z is > 135 and < 225);
                animNoHand.SetBool("MovingSideWaysHand", _eulerAngles.z is > 45 and < 135);
                animNoHand.SetBool("MovingSideWaysNoHand", _eulerAngles.z is > 225 and < 315);
        
                animNoHand.SetFloat("MoveSpeed", rb.linearVelocity.sqrMagnitude);   
            }
        }
        else if (playerNoHandVisual.activeSelf)
        {
            animNoHand.SetFloat("MoveSpeed", 0);
        }
    }


    #endregion
    
    #region Abilities

    private IEnumerator StartWeaponAbility()
    {
        canGetAbilityGain = false;
        InGameUIManager.Instance.pressSpace.SetActive(false);

        switch (myWeapon)
        {
            case MyWeapon.AssaultRifle:
                fastBullets = true;
                break;
            case MyWeapon.Magnum:
                freezeBullets = true;
                break;
            case MyWeapon.Pistol:
                explosiveBullets = true;
                break;
            case MyWeapon.HuntingRifle:
                endlessPenetrationBullets = true;
                break;
            case MyWeapon.Shotgun:
                stickyBullets = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        float _abilityTimer = maxAbilityTime;
        while (_abilityTimer > 0)
        {
            _abilityTimer -= Time.deltaTime;
            currentAbilityTime = _abilityTimer; 
            yield return null; 
        }
        
        fastBullets = false;
        freezeBullets = false;
        explosiveBullets = false;
        endlessPenetrationBullets = false;
        stickyBullets = false;
        
        canGetAbilityGain = true;
    }
    
    #endregion

    #region Weapons
    
    private void WeaponTimerUpdate()
    {
        if (weaponVisual.activeSelf)
        {
            currentShootingDelay -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Sets every value of the weaponSO object to the player values.
    /// </summary>
    /// <param name="weapon"></param>
    public void GetWeapon(WeaponObjectSO weapon)
    {
        weaponVisual.SetActive(true);
        weaponVisual.GetComponent<SpriteRenderer>().sprite = weapon.inGameWeaponVisual;
        bulletDamage = weapon.bulletDamage;
        maxPenetrationCount = weapon.penetrationCount;
        maxShootingDelay = weapon.shootDelay;
        weaponAccuracy = weapon.weaponSpread;
        weaponVisual.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.knockBack;
        maxClipSize = weapon.clipSize;
        ammunitionInBackUp = weapon.ammunitionInBackUp;
        ammunitionInClip = weapon.ammunitionInClip;
        SetAmmunitionText(weapon.ammunitionInClip.ToString(), weapon.ammunitionInBackUp.ToString());
        foreach (var _bullet in BulletPoolingManager.Instance.GetBulletList())
        {
            _bullet.transform.localScale = weapon.bulletSize;
        }
        weaponScreenShake = weapon.screenShake;
        enemyShootingKnockBack = weapon.enemyKnockBackPerBullet;

        playerVisual.SetActive(false);
        playerNoHandVisual.SetActive(true);
        hasWeapon = true;

        InGameUIManager.Instance.equippedWeapon.GetComponent<Image>().sprite = weapon.inGameWeaponVisual;
        InGameUIManager.Instance.equippedWeapon.SetActive(true);

        myWeapon = weapon.weaponName switch
        {
            "Magnum magnum" => MyWeapon.Magnum,
            "French Fries AR" => MyWeapon.AssaultRifle,
            "Lollipop Shotgun" => MyWeapon.Shotgun,
            "Corn Dog Hunting Rifle" => MyWeapon.HuntingRifle,
            "Popcorn Pistol" => MyWeapon.Pistol,
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
    
    #endregion
    
    #region Interaction
    
    public bool GetInteractionObjectInRange(LayerMask layer, out Collider2D interactable)
    {
        interactable = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactable != null;
    }
    
    private void HandleInteractionIndicator()
    {
        if (GetInteractionObjectInRange(wheelOfFortuneLayer, out _) ||
            GetInteractionObjectInRange(collectibleLayer, out _) ||
            GetInteractionObjectInRange(rideLayer, out Collider2D _ride) && _ride.GetComponent<Ride>().canActivateRide ||
            GetInteractionObjectInRange(generatorLayer, out Collider2D _generator) && _generator.GetComponent<Generator>().isInteractable)
        {
            canInteract = true;
        }
        else
        {
            canInteract = false;
        }
    }

    #endregion

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}