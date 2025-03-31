using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    [Header("Travel")]
    private const float BulletFlyingTimeUntilDestroy = 7;
    private float flyingTime;
    private Vector2 travelDirection;
    private Rigidbody2D rb;
    private TrailRenderer trailRenderer;

    [Header("Ability")]
    [SerializeField] private float stickyBulletTimer;
    [SerializeField] private float maxStickyBulletTimer = 2;
    [SerializeField] private float tickStickyBulletDamage = 2;
    [SerializeField] private float explosiveDamage = 3;
    [SerializeField] private float enemyFreezeTime = 3;
    [FormerlySerializedAs("criticalHuntingRifleDamageProbability")]
    [Tooltip("The probability is one in the number you choose - so 1 in 10 if you chose 10"), Range(1, 100)]
    [SerializeField] private int criticalHuntingRifleDamageProbabilityPercentage = 50;
    [SerializeField] private float maxCriticalHuntingRifleDamageMultiplier = 4;
    [SerializeField] private float minCriticalHuntingRifleDamageMultiplier = 2;
    private int tickCount = 2;

    [Header("Enemy")] 
    [SerializeField] private float knockBackTime = .15f;
    
    private float criticalDamage;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        flyingTime += Time.deltaTime;
        
        DeactivateBulletTooFarAwayUpdate();
        
        StickyBulletsTickDamageUpdate();
    }

    private void DeactivateBulletTooFarAwayUpdate()
    {
        if (flyingTime > BulletFlyingTimeUntilDestroy)
        {
            DeactivateBullet();
        }
    }

    private void StickyBulletsTickDamageUpdate()
    {
        if (PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility == AbilityBehaviour.CurrentAbility.StickyBullets)
        {
            stickyBulletTimer -= Time.deltaTime;

            if (stickyBulletTimer <= 0)
            {
                tickCount--;
                GetComponentInParent<EnemyHealthPoints>().TakeDamage(tickStickyBulletDamage, null);
                GetComponentInParent<EnemyBase>().HitVisual();
                stickyBulletTimer = maxStickyBulletTimer;
                
                if (tickCount <= 0)
                {
                    DeactivateBullet();
                }
            }
        }
    }

    public void LaunchInDirection(PlayerBehaviour shooter, Vector2 shootDirection)
    {
        if (trailRenderer != null)
        {
            trailRenderer.widthMultiplier = transform.localScale.x;
            trailRenderer.emitting = true;
            trailRenderer.Clear();
        }
        
        GetComponent<Rigidbody2D>().AddForce(shootDirection * shooter.weaponBehaviour.bulletSpeed, ForceMode2D.Impulse);

        travelDirection = shootDirection;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints) ||
            !col.gameObject.TryGetComponent(out EnemyBase _enemy))
        {
            DeactivateBullet();
        }
        else
        {
            ApplyAbilities(_enemy);

            _enemy.StartCoroutine(EnemyKnockBack(_enemy));

            DealDamage(_enemyHealthPoints);
        }
    }

    private void ApplyAbilities(EnemyBase enemyBase)
    {
        switch (PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility)
        {
            case AbilityBehaviour.CurrentAbility.FreezeBullets:
                enemyBase.EnemyFreezeTime = enemyFreezeTime;
                break;
            
            case AbilityBehaviour.CurrentAbility.PenetrationBullets:
                if (Random.Range(1, 101) <= criticalHuntingRifleDamageProbabilityPercentage)
                {
                    criticalDamage = Random.Range(PlayerBehaviour.Instance.weaponBehaviour.bulletDamage * minCriticalHuntingRifleDamageMultiplier, 
                        PlayerBehaviour.Instance.weaponBehaviour.bulletDamage * maxCriticalHuntingRifleDamageMultiplier);
                }
                else
                {
                    criticalDamage = PlayerBehaviour.Instance.weaponBehaviour.bulletDamage;
                }
                break;
            
            case AbilityBehaviour.CurrentAbility.StickyBullets:
                transform.position = enemyBase.transform.position;
                rb.linearVelocity = Vector2.zero;
                break;

            case AbilityBehaviour.CurrentAbility.FastBullets:
                break;
            
            case AbilityBehaviour.CurrentAbility.ExplosiveBullets:
                break;
            
            case AbilityBehaviour.CurrentAbility.None:
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private IEnumerator EnemyKnockBack(EnemyBase enemyBase)
    {
        enemyBase.enemyCanMove = false;

        float _knockBackWithEnemyResistance = Mathf.Max(PlayerBehaviour.Instance.weaponBehaviour.enemyShootingKnockBack - flyingTime - enemyBase.knockBackResistance, 0);
        enemyBase.GetComponent<Rigidbody2D>().AddForce(travelDirection * _knockBackWithEnemyResistance, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);

        enemyBase.enemyCanMove = true;
    }

    private void DealDamage(EnemyHealthPoints enemyHealthPoints)
    {
        var _bulletPenetrationCount = PlayerBehaviour.Instance.weaponBehaviour.maxPenetrationCount;
        
        if (PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility != AbilityBehaviour.CurrentAbility.PenetrationBullets)
        {
            _bulletPenetrationCount -= 1;
            enemyHealthPoints.TakeDamage(PlayerBehaviour.Instance.weaponBehaviour.bulletDamage, transform);
        }
        else
        {
            enemyHealthPoints.TakeDamage(criticalDamage, transform);
        }

        if (_bulletPenetrationCount >= 0) 
            return;
        
        if (PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility != AbilityBehaviour.CurrentAbility.StickyBullets)
        {
            DeactivateBullet();
        }
    }
    
    private void DeactivateBullet()
    {
        if (PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility != AbilityBehaviour.CurrentAbility.StickyBullets)
        {
            var _bulletImpactParticles = BulletPoolingManager.Instance.impactParticles;
            _bulletImpactParticles.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);

            _bulletImpactParticles.Play();
        }

        if (PlayerBehaviour.Instance.abilityBehaviour.currentActiveAbility == AbilityBehaviour.CurrentAbility.ExplosiveBullets)
        {
            AudioManager.Instance.Play("PopCornExplosion");

            var _popCornParticles = BulletPoolingManager.Instance.popcornParticles;
            _popCornParticles.transform.position = gameObject.transform.position;
            _popCornParticles.Play();
            Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(transform.position, 3);

            foreach (var _enemy in _hitEnemies)
            {
                if (_enemy.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints))
                {
                    _enemyHealthPoints.TakeDamage(explosiveDamage, transform);
                }
            }
        }
        
        trailRenderer.Clear();
        trailRenderer.emitting = false;
        gameObject.SetActive(false);
    }
}
