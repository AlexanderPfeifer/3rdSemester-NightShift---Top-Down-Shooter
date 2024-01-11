using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, SavableVector3> positionBySceneName = new Dictionary<string, SavableVector3>();
    }

    [Header("Scripts")]
    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] public Bullet bulletPrefab;
    [SerializeField] private PlayerSaveData playerSaveData;
    [SerializeField] private Ride ride;
    
    [Header("Floats")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxAbilityTime = 10f;
    [SerializeField] public float shootingSpread = .5f;
    [SerializeField] public float maxShootDelay = 0.3f;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Layers")]
    [SerializeField] private LayerMask wheelOfFortuneLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] private LayerMask collectibleLayer;
    [SerializeField] private LayerMask rideLayer;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] private GameObject fortuneWheelUI;
    [SerializeField] private GameObject generatorUI;

    [Header("Transforms")]
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    
    [Header(("Bullet"))]
    [SerializeField] public GameObject weaponVisual;
    [SerializeField] public float bulletDamage;
    [SerializeField] public int maxPenetrationCount;
    [SerializeField] private float shootDelay;
    [SerializeField] public float activeAbilityGain;
    [SerializeField] public float bulletSpeed = 38f;
    [SerializeField] public float abilityProgress;
    [SerializeField] public int bulletsPerShot;

    [SerializeField] private Animator anim;
    [SerializeField] private Animator animNoHand;

    [SerializeField] public GameObject playerNoHandVisual;
    [SerializeField] public GameObject playerVisual;
    
    private Transform weaponVisualStartPos;
    private Vector3 weaponToMouse;
    private Vector3 mousePos;
    
    private float interactRadius = 2;
    [HideInInspector] public bool isInteracting;
    [HideInInspector] public bool generatorIsActive;
    [HideInInspector] public bool fortuneWheelGotUsed;
    
    private IEnumerator assaultRifleAbilityCoroutine;
    private float currentAbilityTime;
    private bool isUsingAbility;
    [SerializeField] public List<WeaponObjectSO> allWeaponPrizes;

    private Vector2 moveDirection = Vector2.down;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    
    public Action AbilityFunction;

    public bool freezeBullets;
    public bool stickyBullets;
    public bool explosiveBullets;
    public bool endlessPenetrationBullets;
    public bool splitBullets;
    
    private bool weaponAbility;
    FortuneWheelUI rouletteUI;
    private bool canShoot;

    private void Awake()
    {
        GetWeapon();
        
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
        if (playerSaveData.positionBySceneName.TryGetValue(gameObject.scene.name, out var position))
            transform.position = position;
    }
    
    private void LateUpdate()
    {
        //we have to save the current position dependant on the scene the player is in.
        //this way, the position can be retained across multiple scenes, and we can switch back and forth.
        var sceneName = gameObject.scene.name;
        if (!playerSaveData.positionBySceneName.ContainsKey(sceneName))
            playerSaveData.positionBySceneName.Add(sceneName, transform.position);
        else
            playerSaveData.positionBySceneName[sceneName] = transform.position;
        
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
        sr = GetComponentInChildren<SpriteRenderer>();
        muzzleFlashVisual.SetActive(false);
        rouletteUI = fortuneWheelUI.GetComponentInChildren<FortuneWheelUI>();
    }

    private void Update()
    { 
        HandleAimingUpdate();

        AbilityTimeUpdate();

        if (canShoot)
        {
            ShootAutomatic();
        }
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }

    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if (InGameUI.instance.gameIsPaused || isUsingAbility || !weaponVisual.activeSelf)
        {
            canShoot = false;
            return;   
        }

        if (canShoot)
        {
            canShoot = false;
        }
        else
        {
            canShoot = true;
        }
    }

    private void ShootAutomatic()
    {
        if (canShoot && shootDelay <= 0)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Vector2 bulletDirection = Random.insideUnitCircle;
                bulletDirection.Normalize();

                bulletDirection = Vector3.Slerp(bulletDirection, mousePos, 1.0f - shootingSpread);
            
                var newBullet = Instantiate(bulletPrefab, weaponEndPoint.position, Quaternion.identity);
            
                newBullet.Launch(this, bulletDirection);
            
                StartCoroutine(WeaponVisualCoroutine());
            
                shootDelay = maxShootDelay;    
            }
            
        }
    }

    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        if (InGameUI.instance.isActiveAndEnabled)
        {
            InGameUI.instance.PauseGame();
        }
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
                SearchInteractionObject(rideLayer).GetComponent<Ride>().StartNextWave();
            }
        }

        if (CanInteract(collectibleLayer))
        {
            SearchInteractionObject(collectibleLayer).GetComponent<Collectible>().Collect();
        }
    }
    
    private void GameInputManagerOnUsingAbilityAction(object sender, EventArgs e)
    {
        if (!weaponVisual.activeSelf) 
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
        
        if (Vector3.Distance(transform.position, mousePos) <= 1)
        {
            Vector3 mouseOffsetDirection = mousePos * 8 - weaponEndPoint.transform.position;
            mousePos = mouseOffsetDirection;
        }

        weaponToMouse = mousePos - weaponEndPoint.transform.position;
        
        Vector3 newUp = Vector3.Slerp(weaponPosParent.transform.up, weaponToMouse, Time.deltaTime * 10);


        float angle = Vector3.SignedAngle(Vector3.up, newUp, Vector3.forward);
        
        weaponPosParent.eulerAngles = new Vector3(0, 0, angle);

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
        if (!isInteracting && !InGameUI.instance.gameIsPaused)
        { 
            rb.AddForce(new Vector2(gameInputManager.GetMovementVectorNormalized().x, gameInputManager.GetMovementVectorNormalized().y) * moveSpeed, ForceMode2D.Force);
        }
    }

    #region AbilityRegion

        private void AbilityTimeUpdate()
    {
        if (!weaponVisual.activeSelf) 
            return;

        currentAbilityTime -= Time.deltaTime;

        shootDelay -= Time.deltaTime;
    }

    public void StartAssaultRifleAbility()
    {
        StartCoroutine(AssaultRifleAbility());
    }
    
    public void StartMagnumAbility()
    {
        StartCoroutine(MagnumAbility());
    }
    
    public void StartHuntingRifleAbility()
    {
        StartCoroutine(HuntingRifleAbility());
    }
    
    public void StartShotgunAbility()
    {
        StartCoroutine(ShotgunAbility());
    }
    
    public void StartPistolAbility()
    {
        StartCoroutine(PistolAbility());
    }

    private IEnumerator AssaultRifleAbility()
    {
        while (currentAbilityTime > 0)
        {
            splitBullets = true;
            yield return null;
        }

        splitBullets = false;
    }
    
    private IEnumerator MagnumAbility()
    {
        while (currentAbilityTime > 0)
        {
            freezeBullets = true;
            yield return null;
        }

        freezeBullets = false;
    }
    
    private IEnumerator HuntingRifleAbility()
    {
        while (currentAbilityTime > 0)
        {
            endlessPenetrationBullets = true;
            yield return null;
        }

        endlessPenetrationBullets = false;
    }
    
    private IEnumerator ShotgunAbility()
    {
        while (currentAbilityTime > 0)
        {
            stickyBullets = true;
            yield return null;
        }

        stickyBullets = false;
    }
    
    private IEnumerator PistolAbility()
    {
        while (currentAbilityTime > 0)
        {
            explosiveBullets = true;
            yield return null;
        }

        explosiveBullets = false;
    }

    #endregion

    #region GetWeaponAtGameStart

    private void GetWeapon()
    {
        for (int i = 0; i < allWeaponPrizes.Count; i++)
        {
            if (GameSaveStateManager.instance.saveGameDataManager.HasWeaponInInventory(allWeaponPrizes[i].weaponName))
            {
                weaponVisual.GetComponent<SpriteRenderer>().sprite = allWeaponPrizes[i].inGameWeaponVisual;
                weaponVisual.SetActive(true);
                bulletDamage = allWeaponPrizes[i].bulletDamage;
                maxPenetrationCount = allWeaponPrizes[i].penetrationCount;
                maxShootDelay = allWeaponPrizes[i].shootDelay;
                activeAbilityGain = allWeaponPrizes[i].activeAbilityGain;
                shootingSpread = allWeaponPrizes[i].weaponSpread;
                weaponVisual.transform.localScale = allWeaponPrizes[i].weaponScale;
                bulletsPerShot = allWeaponPrizes[i].bulletsPerShot;

                playerVisual.SetActive(false);
                playerNoHandVisual.SetActive(true);

                switch (allWeaponPrizes[i].weaponName)
                {
                    case "Magnum":
                        AbilityFunction = StartMagnumAbility;
                        break;
                    case "Assault Rifle":
                        AbilityFunction = StartAssaultRifleAbility;
                        break;
                    case "Shotgun":
                        AbilityFunction = StartShotgunAbility;
                        break;
                    case "Hunting Rifle":
                        AbilityFunction = StartHuntingRifleAbility;
                        break;
                    case "Pistol":
                        AbilityFunction = StartPistolAbility;
                        break;
                }
            }
            
        }
    }

    #endregion

    private void SetAnimationParameterLateUpdate()
    {
        if (playerVisual.activeSelf)
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
        else
        {
            animNoHand.SetBool("MovingUp", weaponPosParent.eulerAngles.z is < 45 or > 315);
            animNoHand.SetBool("MovingDown", weaponPosParent.eulerAngles.z is > 135 and < 225);
            animNoHand.SetBool("MovingSideWaysHand", weaponPosParent.eulerAngles.z is > 45 and < 135);
            animNoHand.SetBool("MovingSideWaysNoHand", weaponPosParent.eulerAngles.z is > 225 and < 315);
        
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
