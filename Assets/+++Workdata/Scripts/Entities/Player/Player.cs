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
        public string lastOpenedScene;
        public Dictionary<string, SavableVector3> positionBySceneName = new Dictionary<string, SavableVector3>();
    }
    
    [Header("Scripts")]
    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] public Bullet bulletPrefab;
    [SerializeField] private PlayerSaveData playerSaveData;
    
    [Header("Floats")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxAbilityTime = 10f;
    [SerializeField] private float assaultRifleAbilityShootingDelay = 0.1f;
    [SerializeField] private float shootingSpread = .25f;
    [SerializeField] private float maxShootDelay = 0.3f;
    private float shootDelay = 0.3f;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Layers")]
    [SerializeField] private LayerMask wheelOfFortuneLayer;
    [SerializeField] private LayerMask generatorLayer;
    [SerializeField] private LayerMask collectibleLayer;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject muzzleFlashVisual;
    [SerializeField] public GameObject weaponVisual;
    [SerializeField] private GameObject fortuneWheelUI;
    [SerializeField] private GameObject generatorUI;

    [Header("Transforms")]
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    
    private Transform weaponVisualStartPos;
    private Vector3 weaponToMouse;
    private Vector3 mousePos;
    
    private float interactRadius;
    [HideInInspector] public bool isInteracting;
    [HideInInspector] public bool generatorIsActive;
    [HideInInspector] public bool fortuneWheelGotUsed;
    
    private IEnumerator assaultRifleAbilityCoroutine;
    private float currentAbilityTime;
    private bool isUsingAbility;
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    public Action AbilityFunction;
    
    private bool weaponAbility;

    private void Awake()
    {
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
    }

    private void Update()
    { 
        HandleAimingUpdate();

        AbilityTimeUpdate();
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }

    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if (InGameUI.instance.gameIsPaused || isUsingAbility || !weaponVisual.activeSelf || shootDelay > 0)
            return;

        shootDelay = maxShootDelay;
        
        var newBullet = Instantiate(bulletPrefab, weaponEndPoint.transform.position, Quaternion.identity);

        var targetPosition = mousePos;
            
        newBullet.Launch(this, targetPosition);

        StartCoroutine(WeaponVisualCoroutine());
    }

    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        if (InGameUI.instance.isActiveAndEnabled)
        {
            InGameUI.instance.PauseGame();
        }
    }

    private Collider2D CircleCastForInteractionObjects(LayerMask layer)
    {
        var interactionObjectInRange = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactionObjectInRange;
    }
    
    private void GameInputManagerOnInteractAction(object sender, EventArgs e)
    {
        if (CircleCastForInteractionObjects(wheelOfFortuneLayer))
        {
            var rouletteUI = fortuneWheelUI.GetComponentInChildren<FortuneWheelUI>();
            
            if (!fortuneWheelUI.activeSelf && !fortuneWheelGotUsed)
            {
                fortuneWheelUI.SetActive(true);
            }
            else if(fortuneWheelUI.activeSelf && !rouletteUI.canGetPrize)
            {
                fortuneWheelUI.SetActive(false);
            }
        }

        if (CircleCastForInteractionObjects(generatorLayer))
        {
            if (!generatorUI.activeSelf && !generatorIsActive)
            {
                generatorUI.SetActive(true);
                InGameUI.instance.SaveGame();
            }
            else
            {
                generatorUI.SetActive(false);
            }
        }

        if (CircleCastForInteractionObjects(collectibleLayer))
        {
            CircleCastForInteractionObjects(collectibleLayer).GetComponent<Collectible>().Collect();
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
        if (InGameUI.instance.gameIsPaused || !weaponVisual.activeSelf) 
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
    
    private void StartShotgunAbility()
    {
        StartCoroutine(ShotgunAbility());
    }
    
    private void StartMagnumAbility()
    {
        StartCoroutine(MagnumAbility());
    }
    
    private void StartHuntingRifleAbility()
    {
        StartCoroutine(HuntingRifleAbility());
    }
    
    private IEnumerator WeaponVisualCoroutine()
    {
        muzzleFlashVisual.SetActive(true);
        
        yield return new WaitForSeconds(.1f);
        
        muzzleFlashVisual.SetActive(false);
    }

    public IEnumerator AssaultRifleAbility()
    {
        while (currentAbilityTime > 0)
        {
            Vector2 bulletDirection = Random.insideUnitCircle;
            bulletDirection.Normalize();

            bulletDirection = Vector3.Slerp(bulletDirection, mousePos, 1.0f - shootingSpread);
        
            Bullet newBullet = Instantiate(bulletPrefab, weaponEndPoint.position, Quaternion.identity);
            newBullet.Launch(this, bulletDirection);
        
            yield return new WaitForSeconds(assaultRifleAbilityShootingDelay);
        }
    }
    
    private IEnumerator MagnumAbility()
    {
        while (currentAbilityTime > 0)
        {
            
        
            yield return new WaitForSeconds(assaultRifleAbilityShootingDelay);
        }
    }
    
    private IEnumerator HuntingRifleAbility()
    {
        while (currentAbilityTime > 0)
        {
            
        
            yield return new WaitForSeconds(assaultRifleAbilityShootingDelay);
        }
    }
    
    private IEnumerator ShotgunAbility()
    {
        while (currentAbilityTime > 0)
        {
            
        
            yield return new WaitForSeconds(assaultRifleAbilityShootingDelay);
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireCube(mousePos, new Vector3(1, 1, 1));
    }
}
