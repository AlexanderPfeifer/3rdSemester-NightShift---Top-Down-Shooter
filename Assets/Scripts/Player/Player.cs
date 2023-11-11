using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    #region Scripts
    [SerializeField] private GameInputManager gameInputManager;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private MenuUIManager menuUIManager;
    [SerializeField] private AimAt aimAt;
    #endregion

    [SerializeField] private float moveSpeed = 7f;

    private bool mouseCanAim;
    
    private Camera mainCamera;

    [SerializeField] private Transform weaponPosParent;
    [SerializeField] public Transform weaponEndPoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        gameInputManager.OnShootingAction += GameInputManagerOnShootingAction;
        gameInputManager.OnGamePausedAction += GameInputManagerOnGamePausedAction;
    }

    private void Update()
    {
        if(!MenuUIManager.GameIsPaused && mouseCanAim)
        {
            HandleAiming();
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void GameInputManagerOnShootingAction(object sender, EventArgs e)
    {
        if(!MenuUIManager.GameIsPaused)
        {
            var newBullet = Instantiate(bulletPrefab, weaponEndPoint.transform.position, Quaternion.identity);
            
            var targetPosition = aimAt.GetMousePosition();
            
            newBullet.Launch(this, targetPosition);
        }
    }

    private void GameInputManagerOnGamePausedAction(object sender, EventArgs e)
    {
        menuUIManager.PauseGame();
    }

    private void HandleAiming()
    {
        weaponPosParent.right = aimAt.GetMousePosition();
        weaponEndPoint.right = aimAt.GetMousePosition();

        if (weaponPosParent.eulerAngles.z is > 90 and < 270)
        {
            weaponPosParent.GetComponentInChildren<SpriteRenderer>().sortingOrder = sr.sortingOrder - 1;
        }
        else
        {
            weaponPosParent.GetComponentInChildren<SpriteRenderer>().sortingOrder = sr.sortingOrder + 1;
        }
    }

    private void HandleMovement()
    {
        rb.AddForce(new Vector2(gameInputManager.GetMovementVectorNormalized().x, gameInputManager.GetMovementVectorNormalized().y) * moveSpeed, ForceMode2D.Force);
    }

    public Vector2 GetPosition()
    {
        return transform.position;
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

    private void OnTriggerEnter2D(Collider2D col)
    {
        //mouseCanAim = !col.OverlapPoint(aimAt.GetMousePosition());
    }
}
