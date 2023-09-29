using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerChildObjectMovement playerChildObjectMovement;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        rb.AddForce(new Vector2(gameInput.GetMovementVectorNormalized().x, gameInput.GetMovementVectorNormalized().y) * moveSpeed, ForceMode2D.Force);
    }

    public void HandleShooting(InputAction.CallbackContext context)
    {

    }
}
