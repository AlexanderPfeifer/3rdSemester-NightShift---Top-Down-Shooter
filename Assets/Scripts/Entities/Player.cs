using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    #region Scripts

    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private MenuUIManager menuUIManager;

    #endregion

    [SerializeField] private float moveSpeed = 7f;
    
    [SerializeField] private Camera mainCamera;
    private Vector3 mousePos;

    private float interactRadius;
    private LayerMask interactionLayer;

    private Vector3 weaponToMouse;
    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;
    private Transform weaponVisualStartPos;

    [SerializeField] private GameObject muzzleFlashVisual;
    
    
    
    [SerializeField] private float shootingSpread = .25f;
    private float currentAbilityTime;
    [SerializeField] private float maxAbilityTime = 10f;
    [SerializeField] private float shootDelay = 0.1f;
    private IEnumerator AssaultRifleAbilityCoroutine;
    private bool isUsingAbility;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction += GameInputManagerOnInteractAction;
        muzzleFlashVisual.SetActive(false);
        AssaultRifleAbilityCoroutine = AssaultRifleAbility();
    }

    private void Update()
    {
        if(!MenuUIManager.GameIsPaused)
        {
            HandleAimingUpdate();
        }
        
        currentAbilityTime -= Time.deltaTime;

        if (currentAbilityTime <= 0)
        {
            StopCoroutine(AssaultRifleAbilityCoroutine);
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            currentAbilityTime = maxAbilityTime;

            StartCoroutine(AssaultRifleAbilityCoroutine);
        }
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }

    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if(!MenuUIManager.GameIsPaused || !isUsingAbility)
        {
            var newBullet = Instantiate(bulletPrefab, weaponEndPoint.transform.position, Quaternion.identity);

            var targetPosition = mousePos;
            
            newBullet.Launch(this, targetPosition);

            StartCoroutine(WeaponVisualCoroutine());
        }
    }

    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        menuUIManager.PauseGame();
    }
    
    private void GameInputManagerOnInteractAction(object sender, EventArgs e)
    {
        if (Physics2D.OverlapCircle(transform.position, interactRadius,  interactionLayer))
        {

        }
    }

    private void HandleAimingUpdate()
    {
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
        rb.AddForce(new Vector2(gameInputManager.GetMovementVectorNormalized().x, gameInputManager.GetMovementVectorNormalized().y) * moveSpeed, ForceMode2D.Force);
    }

    private IEnumerator WeaponVisualCoroutine()
    {
        muzzleFlashVisual.SetActive(true);
        
        yield return new WaitForSeconds(.1f);
        
        muzzleFlashVisual.SetActive(false);
    }

    private IEnumerator AssaultRifleAbility()
    {
        while (true)
        {
            Vector2 bulletDirection = Random.insideUnitCircle;
            bulletDirection.Normalize();

            bulletDirection = Vector3.Slerp(bulletDirection, mousePos, 1.0f - shootingSpread);
        
            Bullet newBullet = Instantiate(bulletPrefab, weaponEndPoint.position, Quaternion.identity);
            newBullet.Launch(this, bulletDirection);
        
            yield return new WaitForSeconds(shootDelay);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireCube(mousePos, new Vector3(1, 1, 1));
    }
}
