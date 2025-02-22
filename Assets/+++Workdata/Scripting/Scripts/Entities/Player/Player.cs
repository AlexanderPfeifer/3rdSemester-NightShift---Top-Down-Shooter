using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    [SerializeField] public Bullet bulletPrefab;
    [SerializeField] private PlayerSaveData playerSaveData;

    [Header("CharacterMovement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] public GameObject playerNoHandVisual;
    [SerializeField] public GameObject playerVisual;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator animNoHand;
    [SerializeField] private float knockBackDecay = 5f; 
    private bool walkingSound;
    private Vector2 moveDirection = Vector2.down;
    private Rigidbody2D rb;
    private Vector2 knockBack;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<GameObject> cameras;

    [Header("Layers")]
    [SerializeField] public LayerMask wheelOfFortuneLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] public LayerMask rideLayer;
    [SerializeField] public LayerMask duckLayer;
    [SerializeField] private LayerMask collectibleLayer;
    
    [Header("GameObjects")]
    [SerializeField] public GameObject bullets;
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] private GameObject fortuneWheelUI;
    [SerializeField] private GameObject generatorUI;

    [Header("Light")] 
    [SerializeField] public GameObject globalLightObject;

    [Header("Weapon")]
    [SerializeField] public GameObject weaponVisual;
    [SerializeField] public float bulletDamage;
    [SerializeField] public int maxPenetrationCount;
    [SerializeField] private float shootDelay;
    [SerializeField] private ParticleSystem bulletShellsParticle;
    [SerializeField] public float bulletSpeed = 38f;
    [SerializeField] public int bulletsPerShot;
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    [SerializeField] public float shootingSpread;
    [SerializeField] public float maxShootDelay;
    [SerializeField] public List<WeaponObjectSO> allWeaponPrizes;
    [SerializeField] private Animator weaponAnim;
    [SerializeField] private float weaponToMouseSmoothness = 8;
    public Vector3 mousePos;
    public bool freezeBullets;
    public bool stickyBullets;
    public bool explosiveBullets;
    public bool endlessPenetrationBullets;
    public bool splitBullets;
    public bool canShoot;
    public float shootingKnockBack = 2;
    public float enemyShootingKnockBack = 2;
    private float weaponAngle;
    private Vector3 weaponToMouse;
    private Vector3 changingWeaponToMouse;
    private bool hasWeapon;
    private float weaponScreenShake;

    [Header("Ability")]
    [SerializeField] public float maxAbilityTime;
    public float currentAbilityTime;
    private bool isUsingAbility;
    public bool canGetAbilityGain = true;

    [Header(("Interaction"))]
    [SerializeField] private float interactRadius = 2;
    [HideInInspector] public bool isPlayingDialogue;
    public bool playerCanInteract;
    public bool isInteracting;
    public int rideCount;

    [Header("UI")]
    [SerializeField] private GameObject weaponDecisionUI;
    [SerializeField] private GameObject weaponDecisionWeaponImage;
    [SerializeField] private TextMeshProUGUI weaponDecisionWeaponAbilityText;
    [SerializeField] private TextMeshProUGUI weaponDecisionWeaponName;
    [SerializeField] private GameObject firstSelectedWeaponDecision;
    
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
    
    /*
     we have to save the current position dependant on the scene the player is in.
     this way, the position can be retained across multiple scenes, and we can switch back and forth.
    */
    private void LateUpdate()
    {
        playerSaveData.PositionBySceneName[gameObject.scene.name] = transform.position;
        
        SetAnimationParameterLateUpdate();
    }

    private void OnEnable()
    {
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnNotShootingAction += GameInputManagerOnNotShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction += GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction += GameInputManagerOnUsingAbilityAction;
        gameInputManager.OnSprintingAction += GameInputManagerOnSprintingAction;
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
        InGameUI.Instance.loadingScreenAnim.SetTrigger("End");
        rb = GetComponent<Rigidbody2D>();
        muzzleFlashVisual.SetActive(false);
        InGameUI.Instance.dialogueCount = 0;
        InGameUI.Instance.ActivateInGameUI();
    }

    private void Update()
    { 
        HandleAimingUpdate();

        WeaponTimerUpdate();
        
        ShootAutomatic();

        HandleInteractionIndicator();
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }
    
    private void HandleInteractionIndicator()
    {
        if (GetInteractionObjectInRange(wheelOfFortuneLayer, out _) ||
            GetInteractionObjectInRange(collectibleLayer, out _) ||
            GetInteractionObjectInRange(rideLayer, out Collider2D _ride) && _ride.GetComponent<Ride>().canActivateRide ||
            GetInteractionObjectInRange(generatorLayer, out Collider2D _generator) && _generator.GetComponent<Generator>().isInteractable)
        {
            playerCanInteract = true;
        }
        else
        {
            playerCanInteract = false;
        }
    }

    //Manages shooting action for multiple things like skipping dialogue
    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if (!InGameUI.Instance.inventoryIsOpened && !isUsingAbility && weaponVisual.activeSelf && !isInteracting && !isPlayingDialogue)
        {
            canShoot = true;
        }

        if (InGameUI.Instance.textIsPlaying)
        {
            InGameUI.Instance.textDisplaySpeed = 0;
        }
        
        if (InGameUI.Instance.canPlayNext)
        {
            InGameUI.Instance.PlayNext();
        }

        if (InGameUI.Instance.canEndDialogue)
        {
            InGameUI.Instance.EndDialogue();
        }
    }

    private void GameInputManagerOnNotShootingAction(object sender, EventArgs e)
    {
        canShoot = false;
    }

    //With this method, you can hold down shoot button and it shoots automatically
    private void ShootAutomatic()
    {
        if (canShoot && shootDelay <= 0)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Vector2 _bulletDirection = Random.insideUnitCircle;
                _bulletDirection.Normalize();

                _bulletDirection = Vector3.Slerp(_bulletDirection, weaponToMouse.normalized, 1.0f - shootingSpread);
                
                var newBullet = Instantiate(bulletPrefab, weaponEndPoint.position, Quaternion.Euler(0, 0 ,weaponAngle), bullets.transform);
                newBullet.LaunchInDirection(this, _bulletDirection);
            }

            if (!splitBullets)
            {
                shootDelay = maxShootDelay;
            }
            else
            {
                shootDelay = 0.075f;
            }

            StartCoroutine(WeaponVisualCoroutine());

            knockBack = -weaponToMouse.normalized * shootingKnockBack;
        }
    }

    //Changes movement speed when shift is being held down
    private void GameInputManagerOnSprintingAction(object sender, EventArgs e)
    {
        
    }

    //Opens inventory when pressed
    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        if (InGameUI.Instance.isActiveAndEnabled)
        {
            InGameUI.Instance.PauseGame();
        }
    }

    //Closes Generator ui 
    public void CloseGen()
    {
        generatorUI.SetActive(false);
    }

    //Handles every interaction with objects, for this i made a method with an overlap circle which returns a bool if player can interact
    //Then i made a method for getting the object in interaction range
    private void GameInputManagerOnInteractAction(object sender, EventArgs e)
    {
        if (GetInteractionObjectInRange(wheelOfFortuneLayer, out Collider2D _wheelOfFortune))
        {
            if (!fortuneWheelUI.activeSelf)
            {
                foreach (var _weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
                {
                    weaponDecisionUI.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(firstSelectedWeaponDecision);
                    weaponDecisionWeaponAbilityText.text = "";
                    weaponDecisionWeaponName.text = "";
                    weaponDecisionWeaponImage.GetComponent<Image>().sprite = _weapon.inGameWeaponVisual;
                    weaponDecisionWeaponAbilityText.text += "Special Ability:" + "\n" + _weapon.weaponAbilityDescription;
                    weaponDecisionWeaponName.text += _weapon.weaponName;
                    isInteracting = true;
                    hasWeapon = true;
                }

                if (!hasWeapon)
                {
                    fortuneWheelUI.SetActive(true);
                }
            }
        }

        if (GetInteractionObjectInRange(generatorLayer, out Collider2D _generator))
        {
            if (!generatorUI.activeSelf)
            {
                if (_generator.GetComponent<Generator>().isInteractable)
                {
                    generatorUI.SetActive(true);
                    InGameUI.Instance.SaveGame();
                }
            }
        }
        
        if (GetInteractionObjectInRange(duckLayer, out Collider2D _duckStand))
        {
            AudioManager.Instance.Play("DuckSound");
        }
        
        if (GetInteractionObjectInRange(rideLayer, out Collider2D _interactable))
        {
            var _ride = _interactable.gameObject.GetComponent<Ride>();
            
            if (_ride.canActivateRide)
            {
                _ride.StartWave();
            }
        }
            
        if (GetInteractionObjectInRange(collectibleLayer, out Collider2D _collectible))
        {
            _collectible.GetComponent<Collectible>().Collect();
        }
    }

    //For the decision of keeping a weapon, just closes down the weapon picking ui and goes on with the fight
    public void KeepWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (GetInteractionObjectInRange(wheelOfFortuneLayer, out Collider2D _interactable))
        {
            var _fortuneWheel = _interactable.GetComponent<FortuneWheel>();
            _fortuneWheel.ride.GetComponent<Ride>().canActivateRide = true;
            _fortuneWheel.DeactivateFortuneWheel();   
        }
        weaponDecisionUI.SetActive(false);
        isInteracting = false;
    }
    
    //For the decision of changing the weapon, searches through every weapon and removes in from inventory list
    public void ChangeWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);

        foreach (var _weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GameSaveStateManager.Instance.saveGameDataManager.weaponsInInventoryIdentifiers.Remove(_weapon.name);
            InGameUI.Instance.inventoryWeapon.SetActive(true);
        }

        weaponDecisionUI.SetActive(false);
        fortuneWheelUI.SetActive(true);
    }
    
    //Handles everything for the ability, I set the abilityFunction according to the methods of the weapons and when ability is ready it can be 
    //called with the vignette setting active and a timer going down
    private void GameInputManagerOnUsingAbilityAction(object sender, EventArgs e)
    {
        if (currentAbilityTime >= maxAbilityTime && InGameUI.Instance.fightScene.activeSelf)
        {
            StartCoroutine(StartWeaponAbility());
        }
    }
    
    //handles everything for aiming, I set the screen to world point for the mouse position, the I made a code block for when 
    //the player goes near the character with the mouse, so it wouldn't start glitching around when values of weapon position and
    //mouse position meet each other.
    
    //Then I set the weapon parent to look in the direction where the mouse is and when the weapon is behind the head of the character
    //then the sprite order changes
    private void HandleAimingUpdate()
    {
        if (InGameUI.Instance.inventoryIsOpened) 
            return;
        
        mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        if (Vector3.Distance(transform.position, mousePos) >= 1.35f)
        {
            if (Vector3.Distance(weaponEndPoint.position, mousePos) <= 2f)
            {
                changingWeaponToMouse = mousePos - weaponEndPoint.transform.position;

                Vector3 _negativeMousePositionXY = changingWeaponToMouse * -2;
                Vector3 _positiveMousePositionXY = changingWeaponToMouse * 2;
            
                if (weaponToMouse.x < 0)
                {
                    weaponToMouse = _negativeMousePositionXY.normalized;
                }

                if (weaponToMouse.x > 0)
                {
                    weaponToMouse = _positiveMousePositionXY.normalized;
                }
            }
            else
            {
                weaponToMouse = mousePos - weaponEndPoint.transform.position;
            }
        }

        Vector3 _newUp = Vector3.Slerp(weaponPosParent.transform.up, weaponToMouse, Time.deltaTime * weaponToMouseSmoothness);

        weaponAngle = Vector3.SignedAngle(Vector3.up, _newUp, Vector3.forward);
        
        weaponPosParent.eulerAngles = new Vector3(0, 0, weaponAngle);

        if (!weaponVisual.activeSelf) return;
        
        if (weaponPosParent.eulerAngles.z is > 0 and < 180)
        {
            weaponPosParent.GetComponentInChildren<SpriteRenderer>().sortingOrder = - 1;
        }
        else
        {
            weaponPosParent.GetComponentInChildren<SpriteRenderer>().sortingOrder = + 1;
        }
    }

    //Handles movement from game input read value. Also handles sound when the player is moving
    private void HandleMovementFixedUpdate()
    {
        if (!isPlayingDialogue && !InGameUI.Instance.inventoryIsOpened && !isInteracting)
        { 
            var _targetVelocity = gameInputManager.GetMovementVectorNormalized() * moveSpeed;
            
            rb.linearVelocity = _targetVelocity + knockBack;

            knockBack = Vector2.Lerp(knockBack, Vector2.zero, Time.fixedDeltaTime * knockBackDecay);
            
            if (gameInputManager.GetMovementVectorNormalized().x != 0 ||
                gameInputManager.GetMovementVectorNormalized().y != 0)
            {
                if (!walkingSound)
                {
                    AudioManager.Instance.Play("Walking");
                    walkingSound = true;
                }
            }
            else
            {
                AudioManager.Instance.Stop("Walking");
                walkingSound = false;
            }
        }
    }

    #region Abilities

    private IEnumerator StartWeaponAbility()
    {
        canGetAbilityGain = false;
        InGameUI.Instance.pressSpace.SetActive(false);

        switch (myWeapon)
        {
            case MyWeapon.AssaultRifle:
                splitBullets = true;
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
        
        splitBullets = false;
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
            shootDelay -= Time.deltaTime;
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
        maxShootDelay = weapon.shootDelay;
        shootingSpread = weapon.weaponSpread;
        weaponVisual.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.knockBack;
        bulletPrefab.transform.localScale = weapon.bulletSize;
        weaponScreenShake = weapon.screenShake;
        enemyShootingKnockBack = weapon.enemyKnockBackPerBullet;

        playerVisual.SetActive(false);
        playerNoHandVisual.SetActive(true);
        hasWeapon = true;

        InGameUI.Instance.inventoryWeapon.GetComponent<Image>().sprite = weapon.inGameWeaponVisual;
        InGameUI.Instance.inventoryWeapon.SetActive(true);

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
        
        foreach (var _cam in cameras.Where(cam => cam.GetComponent<CinemachineVirtualCamera>().Priority > 10))
        {
            _cam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = weaponScreenShake;
        }
        
        bulletShellsParticle.Play();

        weaponAnim.SetTrigger("ShootGun");
        
        AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        weaponAnim.SetTrigger("GunStartPos");

        bulletShellsParticle.Stop();

        foreach (var _cam in cameras.Where(cam => cam.GetComponent<CinemachineVirtualCamera>().Priority > 10))
        {
            _cam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        }

        muzzleFlashVisual.SetActive(false);
    }
    
    #endregion
    
    private void SetAnimationParameterLateUpdate()
    {
        if (!isInteracting && !isPlayingDialogue)
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
            AudioManager.Instance.Stop("Walking");
        }
    }
    
    public bool GetInteractionObjectInRange(LayerMask layer, out Collider2D interactable)
    {
        interactable = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactable != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}