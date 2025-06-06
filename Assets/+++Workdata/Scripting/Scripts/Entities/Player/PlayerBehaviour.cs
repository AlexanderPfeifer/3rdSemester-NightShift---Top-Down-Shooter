using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerBehaviour : Singleton<PlayerBehaviour>
{
    [Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, SavableVector3> PositionBySceneName = new();
    }
    [SerializeField] private PlayerSaveData playerSaveData;
    
    [Header("Weapon")] 
    [HideInInspector] public WeaponBehaviour weaponBehaviour;
    [HideInInspector] public AbilityBehaviour abilityBehaviour;

    [Header("Currency")] 
    [HideInInspector] public PlayerCurrency playerCurrency;

    [Header("CharacterMovement")] 
    public float baseMoveSpeed = 6.25f;
    public float slowDownSpeed;
    [SerializeField] private float hitVisualTime = .05f;
    [SerializeField] private float hitVignetteFadeInTime = .2f;
    [SerializeField] private float hitVignetteFadeOutTime = .2f;
    [SerializeField] private Volume hitVignette;
    [SerializeField] private float stunTimeOnEnemyCollision = 3;
    [HideInInspector] public bool gotHit;
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
    [SerializeField] private LayerMask rideLayer;
    private bool isPlayerBusy;
    public TextMeshProUGUI ammoText;

    [Header("InteractableHighlight")] 
    [SerializeField] private SpriteRenderer generatorSpriteRenderer;
    [SerializeField] private SpriteRenderer shopSpriteRenderer;
    [SerializeField] private SpriteRenderer rideSpriteRenderer;
    [SerializeField] private Sprite generatorSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite rideSprite;
    [SerializeField] private Sprite generatorSpriteHighlight;
    [SerializeField] private Sprite shopSpriteHighlight;
    [SerializeField] private Sprite rideSpriteHighlight;

    private void SetupFromData()
    {
        if (playerSaveData.PositionBySceneName.TryGetValue(gameObject.scene.name, out var _position))
            transform.position = _position;
    }

    #region MonoBehaviourMethods

    protected override void Awake()
    {
        base.Awake();

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
        GameInputManager.Instance.OnInteractAction += GameInputManagerOnInteractAction;
        AudioManager.Instance.Play("InGameMusic");
    }

    private void OnDisable()
    {
        GameInputManager.Instance.OnInteractAction -= GameInputManagerOnInteractAction;
    }

    private void Start()
    {
        weaponBehaviour = GetComponentInChildren<WeaponBehaviour>();
        abilityBehaviour = GetComponentInChildren<AbilityBehaviour>();
        SceneManager.Instance.loadingScreenAnim.SetTrigger("End");
        AudioManager.Instance.FadeIn("InGameMusic");
        rb = GetComponent<Rigidbody2D>();
        currentMoveSpeed = baseMoveSpeed;
        playerCurrency = GetComponent<PlayerCurrency>();

        if (DebugMode.Instance != null)
        {
            if (DebugMode.Instance.activateRide)
            {
                FindAnyObjectByType<Generator>().GetComponent<Generator>().SetUpFightArena();
            
                transform.position = new Vector3(36, 36, 0);   
            }

            DebugMode.Instance.GetDebugWeapon();
            
            InGameUIManager.Instance.playerHUD.SetActive(true);
            
            InGameUIManager.Instance.currencyUI.GetCurrencyText().gameObject.SetActive(true);
            
            playerCurrency.AddCurrency(DebugMode.Instance.currencyAtStart, true);
        }
    }

    private void Update()
    {
        HandleInteractionSpriteSwitch();
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
        if (GetInteractionObjectInRange(shopLayer, out _))
        {
            InGameUIManager.Instance.OpenShop();
        }
        else if (GetInteractionObjectInRange(generatorLayer, out Collider2D _generator))
        {
            if (_generator.GetComponent<Generator>().interactable)
            {
                InGameUIManager.Instance.SetGeneratorUI();
                GameSaveStateManager.Instance.SaveGame();
            }
        }
        else if (GetInteractionObjectInRange(rideLayer, out Collider2D _ride))
        {
            if (_ride.TryGetComponent(out Ride ride) && !ride.waveStarted && !ride.generator.interactable)
            {
                ride.generator.gateAnim.SetBool("OpenGate", false);
                ride.generator.SetUpFightArena();
            }
        }
        else if (GetInteractionObjectInRange(duckLayer, out _))
        {
            AudioManager.Instance.Play("DuckSound");
        }
    }

    #endregion

    #region Movement
    
    public void StartHitVisual()
    {
        if (gotHit)
            return;
        
        playerNoHandVisual.GetComponent<SpriteRenderer>().color = Color.red;

        StartCoroutine(HitStop());
    }
    
    private IEnumerator HitStop()
    {
        gotHit = true;
        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(hitVisualTime);
        
        Time.timeScale = 1f;
        
        playerNoHandVisual.GetComponent<SpriteRenderer>().color = Color.white;
        
        var _elapsedTime = 0f;

        while (_elapsedTime < hitVignetteFadeInTime)
        {
            hitVignette.weight = Mathf.Lerp(hitVignette.weight, 1, _elapsedTime / hitVignetteFadeInTime);
            
            _elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        currentMoveSpeed = slowDownSpeed;

        foreach (Transform _children in Ride.Instance.enemyParent.transform)
        {
            if (_children.TryGetComponent(out EnemyBase _enemyBase))
            {
                _enemyBase.target = Ride.Instance.transform;
            }
        }
        
        yield return new WaitForSeconds(stunTimeOnEnemyCollision);
        
        currentMoveSpeed = baseMoveSpeed;
        
        _elapsedTime = 0;
        
        while (_elapsedTime < hitVignetteFadeOutTime)
        {
            hitVignette.weight = Mathf.Lerp(hitVignette.weight, 0, _elapsedTime / hitVignetteFadeOutTime);
        
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        gotHit = false;
    }

    private void HandleMovementFixedUpdate()
    {
        if ((IsPlayerBusy() && !InGameUIManager.Instance.dialogueUI.walkieTalkieText.gameObject.activeSelf) || TutorialManager.Instance.isExplainingCurrencyDialogue) 
            return;
        
        rb.linearVelocity = GameInputManager.Instance.GetMovementVectorNormalized() * currentMoveSpeed + weaponBehaviour.currentKnockBack;

        weaponBehaviour.currentKnockBack = Vector2.Lerp(weaponBehaviour.currentKnockBack, Vector2.zero, Time.fixedDeltaTime * knockBackDecay);
    }

    private void SetAnimationParameterLateUpdate()
    {
        if ((IsPlayerBusy() && !InGameUIManager.Instance.dialogueUI.IsDialoguePlaying() && playerNoHandVisual.activeSelf) || TutorialManager.Instance.isExplainingCurrencyDialogue)
        {
            animNoHand.SetFloat("MoveSpeed", 0);

            return;
        }

        if (playerVisual.activeSelf)
        {
            anim.SetFloat("MoveSpeed", rb.linearVelocity.sqrMagnitude);
            moveDirection = GameInputManager.Instance.GetMovementVectorNormalized();
            anim.SetFloat("MoveDirX", moveDirection.x);
            anim.SetFloat("MoveDirY", moveDirection.y);
        }
        else
        {
            if (GameInputManager.Instance.GetMovementVectorNormalized().sqrMagnitude <= 0.01f && weaponBehaviour.bulletsPerShot <= 1)
            {
                weaponBehaviour.currentBulletDirectionSpread = weaponBehaviour.bulletDirectionSpreadStandingStill;
            }
            else
            {
                weaponBehaviour.currentBulletDirectionSpread = weaponBehaviour.bulletDirectionSpread;
            }
            
            var _snapAngle = weaponBehaviour.LastSnappedAngle;
            animNoHand.SetBool("MovingUp", _snapAngle is >= 337.5f or <= 22.5f);
            //right
            animNoHand.SetBool("MovingSideWaysNoHand", _snapAngle is > 225f and < 337.5f);
            animNoHand.SetBool("MovingDown", _snapAngle is >= 157.5f and <= 225f);
            //left
            animNoHand.SetBool("MovingSideWaysHand", _snapAngle is > 22.5f and < 157.5f);
        
            animNoHand.SetFloat("MoveSpeed", rb.linearVelocity.sqrMagnitude);   
        }
    }

    #endregion

    #region Interaction
    
    private void HandleInteractionSpriteSwitch()
    {
        shopSpriteRenderer.sprite = GetInteractionObjectInRange(shopLayer, out _) ? shopSpriteHighlight : shopSprite;

        if (GetInteractionObjectInRange(generatorLayer, out Collider2D _generator))
        {
            if (_generator.TryGetComponent(out Generator _generatorBehaviour) && _generatorBehaviour.interactable)
            {
                generatorSpriteRenderer.sprite = generatorSpriteHighlight;
            }
        }
        else
        {
            generatorSpriteRenderer.sprite = generatorSprite;
        }

        if (GetInteractionObjectInRange(rideLayer, out Collider2D _ride))
        {
            if (_ride.TryGetComponent(out Ride _rideBehaviour) && !_rideBehaviour.waveStarted && !_rideBehaviour.generator.interactable)
            {
                rideSpriteRenderer.sprite = rideSpriteHighlight;
            }
        }
        else
        {
            rideSpriteRenderer.sprite = null;
        }
    }
    
    public bool GetInteractionObjectInRange(LayerMask layer, out Collider2D interactable)
    {
        interactable = Physics2D.OverlapCircle(transform.position, interactRadius, layer);
        return interactable != null;
    }

    public void SetPlayerBusy(bool isBusy)
    {
        isPlayerBusy = isBusy;
    }

    public bool IsPlayerBusy()
    {
        return isPlayerBusy;
    }

    #endregion
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out AmmoDrop _ammoDrop))
        {
            weaponBehaviour.ObtainAmmoDrop(_ammoDrop, 0, false);
        }
        else if (other.gameObject.TryGetComponent(out CurrencyDrop _currencyDrop))
        {
            playerCurrency.AddCurrency(_currencyDrop.currencyCount, false);
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!ammoText.text.Contains(weaponBehaviour.noAmmoString))
        {
            ammoText.text = "";
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}