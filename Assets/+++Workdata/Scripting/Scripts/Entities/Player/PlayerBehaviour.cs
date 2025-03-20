using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public static PlayerBehaviour Instance;
    
    [Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, SavableVector3> PositionBySceneName = new();
    }
    
    [Header("Saving")]
    private PlayerSaveData playerSaveData;
    
    [Header("Weapon")] 
    [HideInInspector] public WeaponBehaviour weaponBehaviour;
    [HideInInspector] public AbilityBehaviour abilityBehaviour;
    
    [Header("Currency")] 
    [HideInInspector] public PlayerCurrency playerCurrency;

    [Header("CharacterMovement")] 
    public float baseMoveSpeed = 6.25f;
    public float slowDownSpeed;
    [SerializeField] private float knockBackDecay = 5f; 
    [HideInInspector] public float currentMoveSpeed;
    private Vector2 moveDirection = Vector2.down;
    private Rigidbody2D rb;
    public GameObject playerVisual;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator animNoHand;
    public GameObject playerNoHandVisual;

    [Header("Light")] 
    [SerializeField] public GameObject globalLightObject;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2;
    [SerializeField] public LayerMask shopLayer;
    [SerializeField] public LayerMask generatorLayer;
    [SerializeField] public LayerMask duckLayer;
    [SerializeField] private LayerMask collectibleLayer;
    [HideInInspector] public bool canInteract = true;
    [HideInInspector] public bool isInteracting;

    private void SetupFromData()
    {
        if (playerSaveData.PositionBySceneName.TryGetValue(gameObject.scene.name, out var _position))
            transform.position = _position;
    }

    #region MonoBehaviourMethods

    private void Awake()
    {
        Instance = this;

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
        GameInputManager.Instance.OnGamePausedAction += InGameUIManager.Instance.OpenInventory;
        GameInputManager.Instance.OnInteractAction += GameInputManagerOnInteractAction;
        //GameInputManager.Instance.OnSprintingAction += GameInputManagerOnSprintingAction;
        AudioManager.Instance.Play("InGameMusic");
    }

    private void OnDisable()
    {
        GameInputManager.Instance.OnGamePausedAction -= InGameUIManager.Instance.OpenInventory;
        GameInputManager.Instance.OnInteractAction -= GameInputManagerOnInteractAction;
        //GameInputManager.Instance.OnSprintingAction -= GameInputManagerOnSprintingAction;
    }

    private void Start()
    {
        weaponBehaviour = GetComponentInChildren<WeaponBehaviour>();
        InGameUIManager.Instance.loadingScreenAnim.SetTrigger("End");
        InGameUIManager.Instance.ActivateInGameUI();
        rb = GetComponent<Rigidbody2D>();
        currentMoveSpeed = baseMoveSpeed;
        playerCurrency = GetComponent<PlayerCurrency>();

        if (DebugMode.Instance.debugMode)
        {
            FindAnyObjectByType<Generator>().GetComponent<Generator>().SetUpFightArena();
            
            transform.position = new Vector3(38, 4, 0);
            
            DebugMode.Instance.GetDebugWeapon();
        }
    }

    private void Update()
    {
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

    private void GameInputManagerOnInteractAction(object sender, EventArgs e)
    {
        if (GetInteractionObjectInRange(collectibleLayer, out Collider2D _collectible))
        {
            _collectible.GetComponent<Collectible>().Collect();
        }
        else if (GetInteractionObjectInRange(shopLayer, out _) && !InGameUIManager.Instance.shopScreen.activeSelf)
        {
            InGameUIManager.Instance.shopScreen.SetActive(true);
        }
        else if (GetInteractionObjectInRange(generatorLayer, out Collider2D _generator) && !InGameUIManager.Instance.generatorScreen.activeSelf)
        {
            if (_generator.GetComponent<Generator>().genInteractable)
            {
                InGameUIManager.Instance.generatorScreen.SetActive(true);
                GameSaveStateManager.Instance.SaveGame();
            }
        }
        else if (GetInteractionObjectInRange(duckLayer, out _))
        {
            AudioManager.Instance.Play("DuckSound");
        }
    }

    #endregion

    #region Movement

    private void HandleMovementFixedUpdate()
    {
        if (InGameUIManager.Instance.dialogueState != InGameUIManager.DialogueState.DialogueNotPlaying || InGameUIManager.Instance.inventoryIsOpened || isInteracting) 
            return;
        
        rb.linearVelocity = GameInputManager.Instance.GetMovementVectorNormalized() * currentMoveSpeed + weaponBehaviour.CurrentKnockBack;

        weaponBehaviour.CurrentKnockBack = Vector2.Lerp(weaponBehaviour.CurrentKnockBack, Vector2.zero, Time.fixedDeltaTime * knockBackDecay);
    }
    
    private void SetAnimationParameterLateUpdate()
    {
        if (!isInteracting && InGameUIManager.Instance.dialogueState == InGameUIManager.DialogueState.DialogueNotPlaying)
        {
            if (playerVisual.activeSelf)
            {
                anim.SetFloat("MoveSpeed", rb.linearVelocity.sqrMagnitude);

                if (GameInputManager.Instance.GetMovementVectorNormalized().sqrMagnitude <= 0)
                {
                    return;
                }
        
                moveDirection = GameInputManager.Instance.GetMovementVectorNormalized();
                anim.SetFloat("MoveDirX", moveDirection.x);
                anim.SetFloat("MoveDirY", moveDirection.y);
            }
            else
            {
                var _eulerAngles = weaponBehaviour.transform.eulerAngles;
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

    #region Interaction
    
    public bool GetInteractionObjectInRange(LayerMask layer, out Collider2D interactable)
    {
        interactable = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactable != null;
    }
    
    private void HandleInteractionIndicator()
    {
        if (GetInteractionObjectInRange(shopLayer, out _) ||
            GetInteractionObjectInRange(collectibleLayer, out _) ||
            GetInteractionObjectInRange(generatorLayer, out Collider2D _generator) && _generator.GetComponent<Generator>().genInteractable)
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
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}