using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
        gameInputManager.OnInteractAction += GameInputManagerOnInteractAction;
        muzzleFlashVisual.SetActive(false);
    }

    private void Update()
    {
        if(!MenuUIManager.GameIsPaused)
        {
            HandleAimingUpdate();
        }
    }

    private void FixedUpdate()
    {
        HandleMovementFixedUpdate();
    }
    
    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if(!MenuUIManager.GameIsPaused)
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
        
        weaponToMouse = mousePos - weaponEndPoint.transform.position;
        
        weaponPosParent.transform.right = weaponToMouse;

        if (weaponPosParent.eulerAngles.z is > 90 and < 270)
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

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
    }

    public void LoadPlayer()
    {
        var save = SaveSystem.LoadPlayer();

        Vector3 position;
        position.x = save.position[0];
        position.y = save.position[1];
        position.z = save.position[2];

        transform.position = position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(weaponEndPoint.transform.position, weaponToMouse);
        
        Gizmos.DrawWireCube(mousePos, new Vector3(1, 1, 1));
    }
}
