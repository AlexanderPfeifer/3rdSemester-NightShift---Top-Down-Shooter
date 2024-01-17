using System;
using System.Collections;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Enemy : MonoBehaviour
{
    private Rigidbody2D rbEnemy;
    private CapsuleCollider2D colliderEnemy;
    [SerializeField] private float otherEnemiesMoveSpeed;
    public bool enemyCanMove = true;
    [SerializeField] private GameObject enemyDeathMark;

    [SerializeField] private float rideAttackDamage = 1;
    
    public float changeColorTime = .075f;

    public float currentEnemyKnockBack;
    public float maxEnemyKnockBack = 10;

    public bool enemyWaiting;

    private Color noAlpha;
    [SerializeField] private bool isBunny;
    [SerializeField] public bool bunnyCanJump;
    [SerializeField] private float bunnyJumpSpeed;

    private int enemyAttackDelay;
    [SerializeField] private int maxEnemyAttackDelay = 1;

    [SerializeField] private float enemyFreezeTime = 3.5f;

    [SerializeField] private float enemyAbilityGain = 1;
    
    private Ride ride;

    private bool isAttacking;
    [SerializeField] private float maxEnemyFreezeTime = 10f;

    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();

        colliderEnemy = GetComponent<CapsuleCollider2D>();
        
        ride = GetComponentInParent<Ride>();

        currentEnemyKnockBack = maxEnemyKnockBack;
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
            currentEnemyKnockBack = maxEnemyKnockBack;
            TargetRide();
        }
        else
        {
            currentEnemyKnockBack /= 2;
        }

        if (isAttacking)
        {
            currentEnemyKnockBack = 0;
        }
    }

    private void TargetRide()
    {
        if (!isBunny)
        {
            rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, ride.transform.position, otherEnemiesMoveSpeed * Time.deltaTime));
        }
        else
        {
            if (bunnyCanJump)
            {
                rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, ride.transform.position, bunnyJumpSpeed * Time.deltaTime));
            }
        }
    }

    private void AttackRide()
    {
        if (isAttacking && enemyAttackDelay <= 0)
        {
            ride.currentRideHealth -= rideAttackDamage;

            enemyAttackDelay = maxEnemyAttackDelay;

            StartCoroutine(ride.ChangeAlphaOnAttack());
        }
    }

    public void EnemyFreeze()
    {
        enemyFreezeTime = maxEnemyFreezeTime;
        
        while (enemyFreezeTime <= 0)
        {
            enemyCanMove = false;
            return;
        }

        enemyCanMove = true;
    }

    public void Stop(float duration)
    {
        if (enemyWaiting)
        {
            return;
        }

        noAlpha.r = 1;
        noAlpha.g = 0;
        noAlpha.b = 0;
        noAlpha.a = 0.3f;
        GetComponent<SpriteRenderer>().color = noAlpha;
        
        Time.timeScale = 0;

        StartCoroutine(EnemyGotHit(duration));
    }
    
    public IEnumerator EnemyGotHit(float duration)
    {
        enemyWaiting = true;

        yield return new WaitForSecondsRealtime(duration);
        
        Time.timeScale = 1f;
        noAlpha.r = 1;
        noAlpha.g = 1;
        noAlpha.b = 1;
        noAlpha.a = 1;
        GetComponent<SpriteRenderer>().color = noAlpha;
        enemyWaiting = false;
    }

    public void BunnyCanJump()
    {
        bunnyCanJump = !bunnyCanJump;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<Ride>())
        {
            isAttacking = true;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        Instantiate(enemyDeathMark, transform.position, Quaternion.identity, FindObjectOfType<Player>().bullets.transform);
        Player player = FindObjectOfType<Player>();
        Physics2D.IgnoreCollision(colliderEnemy, colliderEnemy);

        if (player.canGetAbilityGain)
        {
            player.currentAbilityProgress += enemyAbilityGain;
        }
        //play death particles
    }
}
