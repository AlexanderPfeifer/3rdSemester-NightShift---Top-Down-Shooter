using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, SavableVector3> PositionBySceneName = new Dictionary<string, SavableVector3>();
    }
    
    public static Player Instance;

    [Header("Scripts")]
    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] public Bullet bulletPrefab;
    [SerializeField] private PlayerSaveData playerSaveData;
    private FortuneWheelUI rouletteUI;

    [Header("CharacterMovement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] public GameObject playerNoHandVisual;
    [SerializeField] public GameObject playerVisual;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator animNoHand;
    private bool isSprinting;
    private bool walkingSound;
    private Vector2 moveDirection = Vector2.down;
    private Rigidbody2D rb;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<GameObject> cameras;

    [Header("Layers")]
    [SerializeField] public LayerMask wheelOfFortuneLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] private LayerMask collectibleLayer;
    [SerializeField] public LayerMask rideLayer;
    [SerializeField] public LayerMask duckLayer;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] private GameObject fortuneWheelUI;
    [SerializeField] private GameObject generatorUI;
    [SerializeField] public GameObject bullets;

    [Header("Light")] 
    [SerializeField] public GameObject globalLightObject;

    [Header(("Weapon"))]
    [SerializeField] public GameObject weaponVisual;
    [SerializeField] public float bulletDamage;
    [SerializeField] public int maxPenetrationCount;
    [SerializeField] private float shootDelay;
    [SerializeField] private ParticleSystem bulletShellsParticle;
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
    public bool freezeBullets;
    public bool stickyBullets;
    public bool explosiveBullets;
    public bool endlessPenetrationBullets;
    public bool splitBullets;
    public bool canShoot;
    public float shootingKnockBack = 2;
    public float enemyShootingKnockBack = 2;
    private bool hasWeapon;
    [SerializeField] private Animator weaponAnim;
    private float weaponScreenShake;

    [Header("Ability")]
    [SerializeField] public float maxAbilityTime;
    [SerializeField] public Volume myVolume;
    private Color vignetteColor;
    [HideInInspector] public Vignette playerVignette;
    private const float VignetteAlphaChangeSpeed = 3;
    public float currentAbilityTime;
    private bool isUsingAbility;
    private Action abilityFunction;
    public bool canGetAbilityGain = true;
    private bool isUsingAssaultRifleAbility;
    private bool isUsingShotgunAbility;
    private bool isUsingMagnumAbility;
    private bool isUsingHuntingRifleAbility;
    private bool isUsingPistolAbility;
    private const float UseAbilitySpeed = 2.5f;

    [Header(("Interaction"))]
    [SerializeField] private float interactRadius = 2;
    [HideInInspector] public bool isPlayingDialogue;
    public bool playerCanInteract;
    public bool isInteracting;
    public int rideCount;
    public bool generatorIsActive;

    [Header("UI")]
    [SerializeField] private GameObject weaponDecisionUI;
    [SerializeField] private GameObject weaponDecisionWeaponImage;
    [SerializeField] private TextMeshProUGUI weaponDecisionWeaponAbilityText;
    [SerializeField] private TextMeshProUGUI weaponDecisionWeaponName;
    [SerializeField] private GameObject firstSelectedWeaponDecision;
    
    private void Awake()
    {
        Instance = this;
        
        CheckForWeapons();

        var currentPlayerSaveData = GameSaveStateManager.Instance.saveGameDataManager.newPlayerSaveData;
        if (currentPlayerSaveData != null)
        {
            //if a set of exists, that means we loaded a save and can take over those values.
            playerSaveData = currentPlayerSaveData;
            SetupFromData();
        }
        
        GameSaveStateManager.Instance.saveGameDataManager.newPlayerSaveData = playerSaveData;
    }
    
    //because the player can move from scene to scene, we want to load the position for the scene we are currently in.
    //if the player was never in this scene, we keep the default position the prefab is at.
    private void SetupFromData()
    {
        if (playerSaveData.PositionBySceneName.TryGetValue(gameObject.scene.name, out var position))
            transform.position = position;
    }
    
    //we have to save the current position dependant on the scene the player is in.
    //this way, the position can be retained across multiple scenes, and we can switch back and forth.
    private void LateUpdate()
    {
        var sceneName = gameObject.scene.name;
        if (!playerSaveData.PositionBySceneName.ContainsKey(sceneName))
            playerSaveData.PositionBySceneName.Add(sceneName, transform.position);
        else
            playerSaveData.PositionBySceneName[sceneName] = transform.position;
        
        SetAnimationParameterLateUpdate();
    }

    //Subscribes to events
    private void OnEnable()
    {
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnNotShootingAction += GameInputManagerOnNotShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction += GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction += GameInputManagerOnUsingAbilityAction;
        gameInputManager.OnSprintingAction += GameInputManagerOnSprintingAction;
        gameInputManager.OnNotSprintingAction += GameInputManagerOnNotSprintingAction;
        AudioManager.Instance.Play("InGameMusic");
    }

    //Unsubscribes events
    private void OnDisable()
    {
        gameInputManager.OnShootingAction -= GameInputManagerOnShootingAction;
        gameInputManager.OnNotShootingAction -= GameInputManagerOnNotShootingAction;
        gameInputManager.OnGamePausedAction -= GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction -= GameInputManagerOnInteractAction;
        gameInputManager.OnUsingAbilityAction -= GameInputManagerOnUsingAbilityAction;
        gameInputManager.OnSprintingAction -= GameInputManagerOnSprintingAction;
        gameInputManager.OnNotSprintingAction -= GameInputManagerOnNotSprintingAction;
    }

    private void Start()
    {
        InGameUI.Instance.loadingScreenAnim.SetTrigger("End");
        rb = GetComponent<Rigidbody2D>();
        muzzleFlashVisual.SetActive(false);
        rouletteUI = fortuneWheelUI.GetComponentInChildren<FortuneWheelUI>();
        InGameUI.Instance.dialogueCount = 0;
        InGameUI.Instance.ActivateInGameUI();
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

        HandleInteractionIndicator();
        
        ChangeVignetteColor();

        if (!canGetAbilityGain)
        {
            abilityFunction();
        }

        if(isUsingAssaultRifleAbility)
        {
            StartAssaultRifleAbility();
        }
        else if (isUsingShotgunAbility)
        {
            StartShotgunAbility();
        }
        else if (isUsingMagnumAbility)
        {
            StartMagnumAbility();
        }
        else if (isUsingHuntingRifleAbility)
        {
            StartHuntingRifleAbility();
        }
        else if (isUsingPistolAbility)
        {
            StartPistolAbility();
        }

        if (splitBullets)
        {
            shootDelay = 0.05f;
        }
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }
    
    //Changes vignette color back and forth for a vivid effect
    private void ChangeVignetteColor()
    {
        vignetteColor = new Color(Mathf.PingPong(VignetteAlphaChangeSpeed * Time.time, 1), playerVignette.color.value.g, playerVignette.color.value.b);

        InGameUI.Instance.pressSpace.SetActive(currentAbilityTime >= maxAbilityTime);
    }

    //Handles Interaction Indicator for when the player can interact with something
    private void HandleInteractionIndicator()
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
                Vector2 bulletDirection = Random.insideUnitCircle;
                bulletDirection.Normalize();

                bulletDirection = Vector3.Slerp(bulletDirection, weaponToMouse.normalized, 1.0f - shootingSpread);
                
                var newBullet = Instantiate(bulletPrefab, weaponEndPoint.position, Quaternion.Euler(0, 0 ,weaponAngle), bullets.transform);
                newBullet.LaunchInDirection(this, bulletDirection);
            }
            
            shootDelay = maxShootDelay;

            StartCoroutine(WeaponVisualCoroutine());

            rb.AddForce(-weaponToMouse * shootingKnockBack, ForceMode2D.Impulse);
        }
    }

    //Changes movement speed when shift is being held down
    private void GameInputManagerOnSprintingAction(object sender, EventArgs e)
    {
        moveSpeed = 55;
    }
    
    private void GameInputManagerOnNotSprintingAction(object sender, EventArgs e)
    {
        moveSpeed = 40;
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
        if (CanInteract(wheelOfFortuneLayer))
        {
            if (!fortuneWheelUI.activeSelf)
            {
                foreach (var weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
                {
                    weaponDecisionUI.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(firstSelectedWeaponDecision);
                    weaponDecisionWeaponAbilityText.text = "";
                    weaponDecisionWeaponName.text = "";
                    weaponDecisionWeaponImage.GetComponent<Image>().sprite = weapon.inGameWeaponVisual;
                    weaponDecisionWeaponAbilityText.text += "Special Ability:" + "\n" + weapon.weaponAbilityDescription;
                    weaponDecisionWeaponName.text += weapon.weaponName;
                    isInteracting = true;
                    hasWeapon = true;
                }

                if (!hasWeapon)
                {
                    fortuneWheelUI.SetActive(true);
                }
            }
        }

        if (CanInteract(generatorLayer))
        {
            if (!generatorUI.activeSelf)
            {
                if (SearchInteractionObject(generatorLayer).gameObject.GetComponent<Generator>().isInteractable)
                {
                    generatorUI.SetActive(true);
                    InGameUI.Instance.SaveGame();
                }
            }
        }
        
        if (CanInteract(duckLayer))
        {
            AudioManager.Instance.Play("DuckSound");
        }
        
        if (CanInteract(rideLayer))
        {
            if (SearchInteractionObject(rideLayer).gameObject.GetComponent<Ride>() != null)
            {
                if (SearchInteractionObject(rideLayer).gameObject.GetComponent<Ride>().canActivateRide)
                {
                    SearchInteractionObject(rideLayer).gameObject.GetComponent<Ride>().StartWave();
                }
            }
        }

        if (CanInteract(collectibleLayer))
        {
            SearchInteractionObject(collectibleLayer).GetComponent<Collectible>().Collect();
        }
    }

    //For the decision of keeping a weapon, just closes down the weapon picking ui and goes on with the fight
    public void KeepWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);
        SearchInteractionObject(wheelOfFortuneLayer).GetComponent<FortuneWheel>().ride.GetComponent<Ride>().canActivateRide = true;
        SearchInteractionObject(wheelOfFortuneLayer).GetComponent<FortuneWheel>().DeactivateFortuneWheel();
        weaponDecisionUI.SetActive(false);
        isInteracting = false;
    }
    
    //For the decision of changing the weapon, searches through every weapon and removes in from inventory list
    public void ChangeWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);

        foreach (var weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
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
            currentAbilityTime = maxAbilityTime;

            currentAbilityTime = maxAbilityTime;
            
            playerVignette.active = true;
            vignetteColor = Color.red;
            playerVignette.color.value = vignetteColor;
            
            if (abilityFunction == StartHuntingRifleAbility)
            {
                isUsingHuntingRifleAbility = true;
            }
            else if (abilityFunction == StartMagnumAbility)
            {
                isUsingMagnumAbility = true;
            }
            else if (abilityFunction == StartPistolAbility)
            {
                isUsingPistolAbility = true;
            }
            else if (abilityFunction == StartShotgunAbility)
            {
                isUsingShotgunAbility = true;
            }
            else if (abilityFunction == StartAssaultRifleAbility)
            {
                isUsingAssaultRifleAbility = true;
            }
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

    //Handles movement from game input read value. Also handles sound when the player is moving
    private void HandleMovementFixedUpdate()
    {
        if (!isPlayingDialogue && !InGameUI.Instance.inventoryIsOpened && !isInteracting)
        { 
            rb.AddForce(new Vector2(gameInputManager.GetMovementVectorNormalized().x, gameInputManager.GetMovementVectorNormalized().y) * moveSpeed, ForceMode2D.Force);
            
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

    #region AbilityRegion

    //These are methods for all the abilities and the timer update
    private void WeaponTimerUpdate()
    {
        if (!weaponVisual.activeSelf) 
            return;
        
        shootDelay -= Time.deltaTime;
    }

    private void StartAssaultRifleAbility()
    {
        currentAbilityTime -= Time.deltaTime * UseAbilitySpeed;
        splitBullets = true;
        canGetAbilityGain = false;

        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            splitBullets = false;
            playerVignette.active = false;
            isUsingAssaultRifleAbility = false;
        }
    }
    
    private void StartMagnumAbility()
    {
        freezeBullets = true;
        currentAbilityTime -= Time.deltaTime * UseAbilitySpeed;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            freezeBullets = false;
            playerVignette.active = false;
            isUsingMagnumAbility = false;
        }
    }
    
    private void StartHuntingRifleAbility()
    {
        endlessPenetrationBullets = true;
        currentAbilityTime -= Time.deltaTime * UseAbilitySpeed;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            endlessPenetrationBullets = false;
            playerVignette.active = false;
            isUsingHuntingRifleAbility = false;
        }
    }
    
    private void StartShotgunAbility()
    {
        stickyBullets = true;
        currentAbilityTime -= Time.deltaTime * UseAbilitySpeed;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            stickyBullets = false;
            playerVignette.active = false;
            isUsingShotgunAbility = false;
        }
    }
    
    private void StartPistolAbility()
    {
        explosiveBullets = true;
        currentAbilityTime -= Time.deltaTime * UseAbilitySpeed;
        canGetAbilityGain = false;
        
        if (currentAbilityTime <= 0)
        {
            canGetAbilityGain = true;
            explosiveBullets = false;
            playerVignette.active = false;
            isUsingPistolAbility = false;
        }
    }
    #endregion

    #region GetWeaponAtGameStart

    //Checks for every weapon in the inventory of the player and gets the weapon 
    private void CheckForWeapons()
    {
        foreach (var weapon in allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GetWeapon(weapon);
        }
    }
    
    //This method sets every value of the weapon object so to the player witch ability, weapon perks etc.
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

        switch (weapon.weaponName)
        {
            case "Magnum magnum" :
                abilityFunction = StartMagnumAbility;
                break;
            case "French Fries AR" :
                abilityFunction = StartAssaultRifleAbility;
                break;
            case "Lollipop Shotgun" :
                abilityFunction = StartShotgunAbility;
                break;
            case "Corn Dog Hunting Rifle" :
                abilityFunction = StartHuntingRifleAbility;
                break;
            case "Popcorn Pistol" :
                abilityFunction = StartPistolAbility;
                break;
        }
    }

    #endregion

    //Handles animations for the player like walking speed, direction etc.
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

        if (playerNoHandVisual.activeSelf && isInteracting && isPlayingDialogue)
        {
            animNoHand.SetFloat("MoveSpeed", 0);
            AudioManager.Instance.Stop("Walking");
        }
    }

    //Every time a weapon shoots, the visual of it is called. In this method there is a muzzle flash, a sound, camera shake etc. which is delayed by wait for seconds
    private IEnumerator WeaponVisualCoroutine()
    {
        muzzleFlashVisual.SetActive(true);
        
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i].GetComponent<CinemachineVirtualCamera>().Priority > 10)
            {
                cameras[i].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = weaponScreenShake;
            }
        }
        
        bulletShellsParticle.Play();

        weaponAnim.SetTrigger("ShootGun");
        
        AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        weaponAnim.SetTrigger("GunStartPos");

        bulletShellsParticle.Stop();

        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i].GetComponent<CinemachineVirtualCamera>().Priority > 10)
            {
                cameras[i].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
            }
        }

        muzzleFlashVisual.SetActive(false);
    }

    //The method for knowing if player is in reach of interaction objects
    private bool CanInteract(LayerMask layer)
    {
        var interactionObjectInRange = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactionObjectInRange;
    }
    
    //The method for getting the object in interaction range
    public Collider2D SearchInteractionObject(LayerMask layer)
    {
        var interactionObjectInRange = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactionObjectInRange;
    }

    //Draws interaction gizmo
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireCube(mousePos, new Vector3(1, 1, 1));
        
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
