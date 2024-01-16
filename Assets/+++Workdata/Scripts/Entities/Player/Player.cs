using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, SavableVector3> PositionBySceneName = new Dictionary<string, SavableVector3>();
    }

    [Header("Scripts")]
    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] public Bullet bulletPrefab;
    [SerializeField] private PlayerSaveData playerSaveData;
    FortuneWheelUI rouletteUI;

    [Header("CharacterMovement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] public GameObject playerNoHandVisual;
    [SerializeField] public GameObject playerVisual;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator animNoHand;
    private Vector2 moveDirection = Vector2.down;
    private Rigidbody2D rb;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Layers")]
    [SerializeField] public LayerMask wheelOfFortuneLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] private LayerMask collectibleLayer;
    [SerializeField] public LayerMask rideLayer;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] private GameObject fortuneWheelUI;
    [SerializeField] private GameObject generatorUI;
    [SerializeField] public GameObject bullets;

    [Header(("Weapon"))]
    [SerializeField] public GameObject weaponVisual;
    [SerializeField] public float bulletDamage;
    [SerializeField] public int maxPenetrationCount;
    [SerializeField] private float shootDelay;

    [SerializeField] public float maxAbilityTime;
    [HideInInspector] public float currentAbilityProgress;
    [SerializeField] public float maxAbilityProgress;
    [SerializeField] public Volume myVolume;
    private float currentAbilityTime;
    private bool isUsingAbility;
    private float weaponAngle;

    [SerializeField] public float bulletSpeed = 38f;
    [SerializeField] public int bulletsPerShot;
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    [SerializeField] public float shootingSpread;
    [SerializeField] public float maxShootDelay;
    private Vector3 weaponToMouse;
    private Vector3 changingWeaponToMouse;
    public Vector3 mousePos;
    [SerializeField] public List<WeaponObjectSO> allWeaponPrizes;
    public Action AbilityFunction;
    public bool freezeBullets;
    public bool stickyBullets;
    public bool explosiveBullets;
    public bool endlessPenetrationBullets;
    public bool splitBullets;
    private bool canShoot;
    public float shootingKnockBack = 2;
    public bool canGetAbilityGain = true;
    
    [Header(("Interaction"))]
    [SerializeField] private float interactRadius = 2;
    [HideInInspector] public bool isPlayingDialogue;
    [HideInInspector] public bool generatorIsActive;
    [HideInInspector] public bool fortuneWheelGotUsed;
    public bool playerCanInteract;
    public bool isInteracting;
    
    private Color vignetteColor;
    private Vignette playerVignette;
    private bool changeAlpha;
    private float vignetteAlphaChangeSpeed = 3;

    private void Awake()
    {
        CheckForWeapons();

        var currentPlayerSaveData = GameSaveStateManager.instance.saveGameDataManager.newPlayerSaveData;
        if (currentPlayerSaveData != null)
        {
            //if a set of exists, that means we loaded a save and can take over those values.
            playerSaveData = currentPlayerSaveData;
            SetupFromData();
        }
        
        GameSaveStateManager.instance.saveGameDataManager.newPlayerSaveData = playerSaveData;
    }
    
    private void SetupFromData()
    {
        //because the player can move from scene to scene, we want to load the position for the scene we are currently in.
        //if the player was never in this scene, we keep the default position the prefab is at.
        if (playerSaveData.PositionBySceneName.TryGetValue(gameObject.scene.name, out var position))
            transform.position = position;
    }
    
    private void LateUpdate()
    {
        //we have to save the current position dependant on the scene the player is in.
        //this way, the position can be retained across multiple scenes, and we can switch back and forth.
        var sceneName = gameObject.scene.name;
        if (!playerSaveData.PositionBySceneName.ContainsKey(sceneName))
            playerSaveData.PositionBySceneName.Add(sceneName, transform.position);
        else
            playerSaveData.PositionBySceneName[sceneName] = transform.position;
        
        SetAnimationParameterLateUpdate();
    }

    private void OnEnable()
    {
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction += GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction += GameInputManagerOnUsingAbilityAction;
    }

    private void OnDisable()
    {
        gameInputManager.OnShootingAction -= GameInputManagerOnShootingAction;
        gameInputManager.OnGamePausedAction -= GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction -= GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction -= GameInputManagerOnUsingAbilityAction;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        muzzleFlashVisual.SetActive(false);
        rouletteUI = fortuneWheelUI.GetComponentInChildren<FortuneWheelUI>();
        InGameUI.instance.ActivateInGameUI();
        myVolume.profile.TryGet(out playerVignette);
    }

    private void Update()
    { 
        HandleAimingUpdate();

        WeaponTimerUpdate();

        if (canShoot)
        {
            ShootAutomatic();
        }

        HandleInventoryIndicator();
        
        ChangeVignetteColor();

        if (!canGetAbilityGain)
        {
            AbilityFunction();
        }
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }
    
    private void ChangeVignetteColor()
    {
        if (currentAbilityProgress >= maxAbilityProgress)
        {
            playerVignette.active = true;
            vignetteColor = Color.red;
            playerVignette.color.value = vignetteColor;
            changeAlpha = true;
        }
        else
        {
            changeAlpha = false;
            vignetteColor = new Color(1, playerVignette.color.value.g, playerVignette.color.value.b);
        }
        
        if (changeAlpha)
        {
            vignetteColor = new Color(Mathf.PingPong(vignetteAlphaChangeSpeed * Time.time, 1), playerVignette.color.value.g, playerVignette.color.value.b);
        }
    }

    private void HandleInventoryIndicator()
    {
        if (CanInteract(wheelOfFortuneLayer) || CanInteract(collectibleLayer))
        {
            playerCanInteract = true;
        }
        else if (CanInteract(rideLayer))
        {
            if (SearchInteractionObject(rideLayer).gameObject.GetComponent<Ride>() != null)
            {
                if (SearchInteractionObject(rideLayer).gameObject.GetComponent<Ride>().canActivateRide)
                {
                    playerCanInteract = true;
                }
            }
        }
        else if (CanInteract(generatorLayer) && SearchInteractionObject(generatorLayer).gameObject.GetComponent<Generator>().isInteractable)
        {
            playerCanInteract = true;
        }
        else
        {
            playerCanInteract = false;
        }
    }

    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if (InGameUI.instance.textIsPlaying)
        {
            InGameUI.instance.textDisplaySpeed = 0;
        }

        if (InGameUI.instance.canEndDialogue)
        {
            InGameUI.instance.EndDialogue();
        }
        
        if (InGameUI.instance.gameIsPaused || isUsingAbility || !weaponVisual.activeSelf)
        {
            canShoot = false;
            return;   
        }

        canShoot = !canShoot;
    }

    private void ShootAutomatic()
    {
        if (canShoot && shootDelay <= 0)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Vector2 bulletDirection = Random.insideUnitCircle;
                bulletDirection.Normalize();

                bulletDirection = Vector3.Slerp(bulletDirection, weaponToMouse.normalized, 1.0f - shootingSpread);

                var newBullet = Instantiate(bulletPrefab, weaponEndPoint.position, Quaternion.Euler(0, 0 ,weaponAngle), bullets.transform);
                newBullet.LaunchInDirection(this, bulletDirection);
            
                StartCoroutine(WeaponVisualCoroutine());
            
                shootDelay = maxShootDelay;    
            }
            
            rb.AddForce(-weaponToMouse * shootingKnockBack, ForceMode2D.Impulse);
        }
    }

    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        if (InGameUI.instance.isActiveAndEnabled)
        {
            InGameUI.instance.PauseGame();
        }
    }

    public void CloseGen()
    {
        generatorUI.SetActive(false);
    }

    private void GameInputManagerOnInteractAction(object sender, EventArgs e)
    {
        if (CanInteract(wheelOfFortuneLayer))
        {
            if (!fortuneWheelUI.activeSelf && !fortuneWheelGotUsed)
            {
                fortuneWheelUI.SetActive(true);
            }
            else if(fortuneWheelUI.activeSelf && !rouletteUI.canGetPrize)
            {
                fortuneWheelUI.SetActive(false);
            }
        }

        if (CanInteract(generatorLayer))
        {
            if (!generatorUI.activeSelf && !generatorIsActive)
            {
                if (SearchInteractionObject(generatorLayer).gameObject.GetComponent<Generator>().isInteractable)
                {
                    generatorUI.SetActive(true);
                    InGameUI.instance.SaveGame();
                }
            }
            else
            {
                generatorUI.SetActive(false);
            }
        }
        
        if (CanInteract(rideLayer))
        {
            if(fortuneWheelGotUsed && generatorIsActive)
            {
                SearchInteractionObject(rideLayer).GetComponent<Ride>().StartWave();
            }
        }

        if (CanInteract(collectibleLayer))
        {
            SearchInteractionObject(collectibleLayer).GetComponent<Collectible>().Collect();
        }
    }
    
    private void GameInputManagerOnUsingAbilityAction(object sender, EventArgs e)
    {
        if (currentAbilityProgress < maxAbilityProgress) 
            return;
        
        currentAbilityTime = maxAbilityTime;

        AbilityFunction();
    }
    
    private void HandleAimingUpdate()
    {
        if (InGameUI.instance.gameIsPaused) 
            return;
        
        mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        if (Vector3.Distance(transform.position, mousePos) >= 1.35f)
        {
            if (Vector3.Distance(weaponEndPoint.position, mousePos) <= 2f)
            {
                changingWeaponToMouse = mousePos - weaponEndPoint.transform.position;

                Vector3 negativeMousePositionXY = changingWeaponToMouse * -2;
                Vector3 positiveMousePositionXY = changingWeaponToMouse * 2;
            
                if (weaponToMouse.x < 0)
                {
                    weaponToMouse = negativeMousePositionXY.normalized;
                }

                if (weaponToMouse.x > 0)
                {
                    weaponToMouse = positiveMousePositionXY.normalized;
                }
            }
            else
            {
                weaponToMouse = mousePos - weaponEndPoint.transform.position;
            }
        }

        Vector3 newUp = Vector3.Slerp(weaponPosParent.transform.up, weaponToMouse, Time.deltaTime * 10);


        weaponAngle = Vector3.SignedAngle(Vector3.up, newUp, Vector3.forward);
        
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

    private void HandleMovementFixedUpdate()
    {
        if (!isPlayingDialogue && !InGameUI.instance.gameIsPaused && !isInteracting)
        { 
            rb.AddForce(new Vector2(gameInputManager.GetMovementVectorNormalized().x, gameInputManager.GetMovementVectorNormalized().y) * moveSpeed, ForceMode2D.Force);
        }
    }

    #region AbilityRegion

    private void WeaponTimerUpdate()
    {
        if (!weaponVisual.activeSelf) 
            return;
        
        shootDelay -= Time.deltaTime;
    }

    private void StartAssaultRifleAbility()
    {
        currentAbilityTime -= Time.deltaTime;
        splitBullets = true;
        canGetAbilityGain = false;

        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            splitBullets = false;
            playerVignette.active = false;
        }
    }
    
    private void StartMagnumAbility()
    {
        freezeBullets = true;
        currentAbilityTime -= Time.deltaTime;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            freezeBullets = false;
            playerVignette.active = false;
        }
    }
    
    private void StartHuntingRifleAbility()
    {
        endlessPenetrationBullets = true;
        currentAbilityTime -= Time.deltaTime;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            endlessPenetrationBullets = false;
            playerVignette.active = false;
        }
    }
    
    private void StartShotgunAbility()
    {
        stickyBullets = true;
        currentAbilityTime -= Time.deltaTime;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            stickyBullets = false;
            playerVignette.active = false;
        }
    }
    
    private void StartPistolAbility()
    {
        explosiveBullets = true;
        currentAbilityTime -= Time.deltaTime;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            explosiveBullets = false;
            playerVignette.active = false;
        }
    }
    #endregion

    #region GetWeaponAtGameStart

    private void CheckForWeapons()
    {
        foreach (var weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GetWeapon(weapon);
        }
    }
    
    public void GetWeapon(WeaponObjectSO weapon)
    {
        weaponVisual.GetComponent<SpriteRenderer>().sprite = weapon.inGameWeaponVisual;
        weaponVisual.SetActive(true);
        bulletDamage = weapon.bulletDamage;
        maxPenetrationCount = weapon.penetrationCount;
        maxShootDelay = weapon.shootDelay;
        shootingSpread = weapon.weaponSpread;
        weaponVisual.transform.localScale = weapon.weaponScale;
        bulletsPerShot = weapon.bulletsPerShot;
        shootingKnockBack = weapon.knockBack;

        playerVisual.SetActive(false);
        playerNoHandVisual.SetActive(true);
            
        InGameUI.instance.inventoryWeapon.GetComponent<Image>().sprite = weapon.inGameWeaponVisual;
        InGameUI.instance.inventoryWeapon.SetActive(true);

        switch (weapon.weaponName)
        {
            case "Magnum magnum" :
                AbilityFunction = StartMagnumAbility;
                break;
            case "French Fries Assault Rifle" :
                AbilityFunction = StartAssaultRifleAbility;
                break;
            case "Lollipop Shotgun" :
                AbilityFunction = StartShotgunAbility;
                break;
            case "Corn Dog Hunting Rifle" :
                AbilityFunction = StartHuntingRifleAbility;
                break;
            case "Popcorn Pistol" :
                AbilityFunction = StartPistolAbility;
                break;
        }
    }

    #endregion

    private void SetAnimationParameterLateUpdate()
    {
        if (playerVisual.activeSelf && !isInteracting && !isPlayingDialogue)
        {
            anim.SetFloat("MoveSpeed", rb.velocity.sqrMagnitude);

            if (gameInputManager.GetMovementVectorNormalized().sqrMagnitude <= 0)
            {
                return;
            }
        
            moveDirection = gameInputManager.GetMovementVectorNormalized();
            anim.SetFloat("MoveDirX", moveDirection.x);
            anim.SetFloat("MoveDirY", moveDirection.y);
        }
        
        if(playerNoHandVisual.activeSelf && !isInteracting && !isPlayingDialogue)
        {
            var eulerAngles = weaponPosParent.eulerAngles;
            animNoHand.SetBool("MovingUp", eulerAngles.z is < 45 or > 315);
            animNoHand.SetBool("MovingDown", eulerAngles.z is > 135 and < 225);
            animNoHand.SetBool("MovingSideWaysHand", eulerAngles.z is > 45 and < 135);
            animNoHand.SetBool("MovingSideWaysNoHand", eulerAngles.z is > 225 and < 315);
        
            animNoHand.SetFloat("MoveSpeed", rb.velocity.sqrMagnitude);
        }
    }

    private IEnumerator WeaponVisualCoroutine()
    {
        muzzleFlashVisual.SetActive(true);
        
        yield return new WaitForSeconds(.1f);
        
        muzzleFlashVisual.SetActive(false);
    }

    private bool CanInteract(LayerMask layer)
    {
        var interactionObjectInRange = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactionObjectInRange;
    }
    
    public Collider2D SearchInteractionObject(LayerMask layer)
    {
        var interactionObjectInRange = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactionObjectInRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireCube(mousePos, new Vector3(1, 1, 1));
        
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
