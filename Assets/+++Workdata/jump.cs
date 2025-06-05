using System;
using UnityEngine;

public class jump : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float jumpForce;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameInputManager.Instance.OnShootingAction += JumpJunge;
    }

    private void JumpJunge(object sender, EventArgs e)
    {
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Force);
        animator.SetTrigger("Flip");
    }
}
