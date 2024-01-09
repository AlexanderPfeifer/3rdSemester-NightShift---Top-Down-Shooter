using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Enemy : MonoBehaviour
{
    private Rigidbody2D rbEnemy;
    private CapsuleCollider2D colliderEnemy;
    private SpriteRenderer srEnemy;
    [SerializeField] private LayerMask rideLayer;
    [SerializeField] private float searchRange;
    [SerializeField] private float moveSpeed;

    private int enemyAttackDelay;
    [SerializeField] private int maxEnemyAttackDelay = 1;
    
    private Ride ride;

    private bool isAttacking;

    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();

        colliderEnemy = GetComponent<CapsuleCollider2D>();

        srEnemy = GetComponentInChildren<SpriteRenderer>();
        
        ride = FindObjectOfType<Ride>();
    }

    private void Update()
    {
        TargetRide();
        AttackRide();
    }

    private void TargetRide()
    {
        if (!CircleCastForRide(rideLayer) || isAttacking) 
            return;
        Vector2.MoveTowards(transform.position, ride.transform.position, moveSpeed * Time.deltaTime);
    }

    private void AttackRide()
    {
        if (isAttacking && enemyAttackDelay <= 0)
        {
            ride.currentRideHp -= 1;
            enemyAttackDelay = maxEnemyAttackDelay;
            if (ride.currentRideHp <= 0)
            {
                ride.ResetRide();
            }
        }
    }
    
    private Collider2D CircleCastForRide(LayerMask layer)
    {
        var interactionObjectInRange = Physics2D.OverlapCircle(transform.position, searchRange, layer);
        return interactionObjectInRange;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == rideLayer)
        {
            isAttacking = true;
        }
    }

    private void OnDestroy()
    {
        //play death anim
    }
}
