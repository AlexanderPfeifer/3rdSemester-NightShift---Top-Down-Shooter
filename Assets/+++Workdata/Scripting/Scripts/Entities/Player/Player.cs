using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float knockBackDecay = 5f; 
    private Vector2 moveDirection = Vector2.down;
    private Vector2 knockBack;
    private Rigidbody2D rb;
    [SerializeField] private GameObject playerVisual;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject playerNoHandVisual;
    [SerializeField] private Animator animNoHand;
    
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [FormerlySerializedAs("cameras")] [SerializeField] private List<GameObject> fightAreaCams;

    [Header("Light")] 
    [SerializeField] public GameObject globalLightObject;

    [Header("Bullets")]
    [SerializeField] public GameObject bulletParent;
    [SerializeField] private ParticleSystem bulletShellsParticle;
    [HideInInspector] public float bulletDamage;
    [HideInInspector] public float bulletSpeed = 38f;
    [HideInInspector] public int maxPenetrationCount;
    private int bulletsPerShot;
    private int maxClipSize;
    private int clipSize;
    private int availableAmmunition;

    [Header("Weapon")]
    public List<WeaponObjectSO> allWeaponPrizes;
    public GameObject weaponVisual;
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    [SerializeField] private Animator weaponAnim;
    [SerializeField] private float weaponToMouseSmoothness = 8;
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] private float reloadTime = 2;
    [HideInInspector] public bool freezeBullets;
    [HideInInspector] public bool stickyBullets;
    [HideInInspector] public bool explosiveBullets;
    [HideInInspector] public bool endlessPenetrationBullets;
    [HideInInspector] public bool fastBullets;
    public float enemyShootingKnockBack = 2;
    private float maxShootingDelay;
    private float currentShootingDelay;
    private float weaponSpread;
    private Vector3 changingWeaponToMouse;
    private Vector3 weaponToMouse;
    private Vector3 mousePos;
    private float weaponAngleSmoothed;
    private float weaponAngleUnSmoothed;
    private float shootingKnockBack;
    private float weaponScreenShake;
    private bool shoot;
    private bool hasWeapon;

    [Header("Ability")]
    [SerializeField] public float maxAbilityTime;
    [SerializeField] private float fastBulletsDelay = 0.075f;
    [HideInInspector] public float currentAbilityTime;
    [HideInInspector] public bool canGetAbilityGain = true;
    private bool isUsingAbility;

    [Header("Interaction")]
    [HideInInspector] public bool canInteract = true;
    [HideInInspector] public bool isInteracting;
    [HideInInspector] public int enemyWave;
    [SerializeField] private float interactRadius = 2;
    [SerializeField] public LayerMask wheelOfFortuneLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] public LayerMask rideLayer;
    [SerializeField] public LayerMask duckLayer;
    [SerializeField] private LayerMask collectibleLayer;

    private MyWeapon myWeapon;
    enum MyWeapon
    {
        AssaultRifle,
        Shotgun,
        Magnum,
        Pistol,
        HuntingRifle,
    }
    
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
    
    private void SetupFromData()
    {
        if (playerSaveData.PositionBySceneName.TryGetValue(gameObject.scene.name, out var _position))
            transform.position = _position;
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
    
    /*
     we have to save the current position dependant on the scene the player is in.
     this way, the position can be retained across multiple scenes, and we can switch back and forth.
    */
    private void LateUpdate()
    {
        playerSaveData.PositionBySceneName[gameObject.scene.name] = transform.position;
        
        SetAnimationParameterLateUpdate();
    }
    
    #region Input

    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if (!InGameUIManager.Instance.inventoryIsOpened && !isUsingAbility && weaponVisual.activeSelf && !isInteracting && InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueNotPlaying)
        {
            shoot = true;
        }
        else
        {
            if (InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialoguePlaying)
            {
                InGameUIManager.Instance.textDisplaySpeed = InGameUIManager.Instance.maxTextDisplaySpeed;
            }
            else if (InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueAbleToGoNext)
            {
                InGameUIManager.Instance.PlayNextDialogue();
            }
            else if (InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueAbleToEnd)
            {
                InGameUIManager.Instance.EndDialogue();
            }   
        }
    }

    private void GameInputManagerOnNotShootingAction(object sender, EventArgs e)
    {
        shoot = false;
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
        
    }

    #endregion

    #region Shooting

    private void HandleAimingUpdate()
    {
        if (InGameUIManager.Instance.inventoryIsOpened || !weaponVisual.activeSelf) 
            return;
        
        mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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
        if (shoot && currentShootingDelay <= 0)
        {
            for (int _i = 0; _i < bulletsPerShot; _i++)
            {
                Vector2 _bulletDirection = Random.insideUnitCircle.normalized;

                _bulletDirection = Vector3.Slerp(_bulletDirection, weaponToMouse.normalized, 1.0f - weaponSpread);

                var _bullet = BulletPoolingManager.Instance.GetInactiveBullet();
                _bullet.transform.SetPositionAndRotation(weaponEndPoint.position, Quaternion.Euler(0, 0 ,weaponAngleUnSmoothed));
                _bullet.gameObject.SetActive(true);
                _bullet.LaunchInDirection(this, _bulletDirection);
            }

            clipSize--;

            currentShootingDelay = fastBullets ? fastBulletsDelay : maxShootingDelay;

            StartCoroutine(WeaponVisualCoroutine());

            knockBack = -weaponToMouse.normalized * shootingKnockBack;
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        if (availableAmmunition <= 0 || maxClipSize - clipSize == maxClipSize)
        {
            //return and make some vfx
            yield break;
        }
        
        yield return new WaitForSeconds(reloadTime);
        
        availableAmmunition -= maxClipSize - clipSize;

        if (availableAmmunition < 0)
        {
            clipSize = maxClipSize - Mathf.Abs(availableAmmunition);
        }
    }

    #endregion

    #region Movement

    private void HandleMovementFixedUpdate()
    {
        if (InGameUIManager.Instance.dialogueState != InGameUIManager.DialogueState.DialogueNotPlaying || InGameUIManager.Instance.inventoryIsOpened || isInteracting) 
            return;
        
        rb.linearVelocity = gameInputManager.GetMovementVectorNormalized() * moveSpeed + knockBack;

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
        weaponSpread = weapon.weaponSpread;
        weaponVisual.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.knockBack;
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
        
        foreach (var _cam in fightAreaCams.Where(cam => cam.GetComponent<CinemachineCamera>().Priority > 10))
        {
            _cam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = weaponScreenShake;
        }
        
        bulletShellsParticle.Play();

        weaponAnim.SetTrigger("ShootGun");
        
        AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        weaponAnim.SetTrigger("GunStartPos");

        bulletShellsParticle.Stop();

        foreach (var _cam in fightAreaCams.Where(cam => cam.GetComponent<CinemachineCamera>().Priority > 10))
        {
            _cam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = 0;
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}