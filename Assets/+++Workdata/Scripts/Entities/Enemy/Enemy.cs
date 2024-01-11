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
    private bool enemyCanMove = true;

    private int enemyAttackDelay;
    [SerializeField] private int maxEnemyAttackDelay = 1;

    [SerializeField] private float enemyFreezeTime = 10f;
    
    private Ride ride;

    private bool isAttacking;
    [SerializeField] private float maxEnemyFreezeTime = 10f;

    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();

        colliderEnemy = GetComponent<CapsuleCollider2D>();

        srEnemy = GetComponentInChildren<SpriteRenderer>();

        ride = GetComponentInParent<Ride>();
    }

    private void Update()
    {
        if(!enemyCanMove)
        {
            enemyFreezeTime -= Time.deltaTime;
        }
        
        AttackRide();
    }

    private void FixedUpdate()
    {
        if (enemyCanMove)
        {
            TargetRide();
        }
    }

    private void TargetRide()
    {
        if (isAttacking) 
            return;
        
        rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, ride.transform.position, moveSpeed * Time.deltaTime));
    }

    private void AttackRide()
    {
        if (isAttacking && enemyAttackDelay <= 0)
        {
            ride.currentRideHp -= 1;
            
            if (ride.currentRideHp <= 0)
            {
                ride.ResetRide();
            }
            
            enemyAttackDelay = maxEnemyAttackDelay;
        }
    }

    public void EnemyFreeze()
    {
        while (enemyFreezeTime <= 0)
        {
            enemyCanMove = false;
            return;
        }

        enemyFreezeTime = maxEnemyFreezeTime;
        enemyCanMove = true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<Ride>())
        {
            isAttacking = true;
            Physics2D.IgnoreCollision(colliderEnemy, colliderEnemy);
        }
    }

    private void OnDestroy()
    {
        //play death anim
    }
}
