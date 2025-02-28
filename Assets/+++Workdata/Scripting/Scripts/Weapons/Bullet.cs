using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    [Header("Travel")]
    private const float BulletDistanceUntilDestroy = 25;
    private Vector2 travelDirection;
    private Vector2 startPosition;
    private Rigidbody2D rb;

    [Header("Ability")]
    [SerializeField] private float stickyBulletTimer;
    [SerializeField] private float maxStickyBulletTimer = 2;
    [SerializeField] private float tickStickyBulletDamage = 2;
    [SerializeField] private float explosiveDamage = 3;
    [SerializeField] private float enemyFreezeTime = 3;
    [Tooltip("The probability is one in the number you choose - so 1 in 10 if you chose 10"), Range(1, 10)]
    [SerializeField] private int criticalHuntingRifleDamageProbability = 5;
    [SerializeField] private float maxCriticalHuntingRifleDamageMultiplier = 4;
    [SerializeField] private float minCriticalHuntingRifleDamageMultiplier = 2;
    private int tickCount = 2;
    private bool popCornParticle;

    [Header("Enemy")] 
    [SerializeField] private float knockBackTime = .15f;
    
    private float criticalDamage;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        criticalDamage = Player.Instance.bulletDamage;
    }
    
    private void Update()
    {
        DestroyBulletTooFarAwayUpdate();
        
        StickyBulletsTickDamageUpdate();
    }

    private void DestroyBulletTooFarAwayUpdate()
    {
        if (Vector2.Distance(startPosition, transform.position) >= BulletDistanceUntilDestroy)
        {
            DeactivateBullet();
        }
    }

    private void StickyBulletsTickDamageUpdate()
    {
        if (Player.Instance.stickyBullets)
        {
            stickyBulletTimer -= Time.deltaTime;

            if (stickyBulletTimer <= 0)
            {
                tickCount--;
                GetComponentInParent<EnemyHealthPoints>().TakeDamage(tickStickyBulletDamage);
                GetComponentInParent<Enemy>().HitStop(gameObject.GetComponentInParent<Enemy>().changeColorTime);
                stickyBulletTimer = maxStickyBulletTimer;
                
                if (tickCount <= 0)
                {
                    DeactivateBullet();
                }
            }
        }
    }

    public void LaunchInDirection(Player shooter, Vector2 shootDirection)
    {
        startPosition = transform.position;
        
        GetComponent<Rigidbody2D>().AddForce(shootDirection * shooter.bulletSpeed, ForceMode2D.Impulse);

        travelDirection = shootDirection;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints) ||
            !col.gameObject.TryGetComponent(out Enemy _enemy))
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

    private void ApplyAbilities(Enemy enemy)
    {
        if (Player.Instance.freezeBullets)
        {
            enemy.enemyFreezeTime = enemyFreezeTime;
        }
        else if(Player.Instance.endlessPenetrationBullets)
        {
            var _probability = Random.Range(1, criticalHuntingRifleDamageProbability);
            
            if (_probability == criticalHuntingRifleDamageProbability - 1)
            {
                criticalDamage = Random.Range(Player.Instance.bulletDamage * minCriticalHuntingRifleDamageMultiplier, 
                    Player.Instance.bulletDamage * maxCriticalHuntingRifleDamageMultiplier);
            }
            else
            {
                criticalDamage = Player.Instance.bulletDamage;
            }
        }
        else if (Player.Instance.stickyBullets)
        {
            transform.SetParent(enemy.transform);
            rb.linearVelocity = Vector2.zero;
        }
        else if (Player.Instance.explosiveBullets)
        {
            popCornParticle = true;
        }
    }
    
    private IEnumerator EnemyKnockBack(Enemy enemy)
    {
        if (enemy.currentEnemyKnockBackResistance != 0)
        {
            enemy.enemyCanMove = false;
        
            enemy.GetComponent<Rigidbody2D>().
                AddForce(travelDirection * (Player.Instance.enemyShootingKnockBack / enemy.currentEnemyKnockBackResistance), 
                    ForceMode2D.Impulse);

            yield return new WaitForSecondsRealtime(knockBackTime);

            enemy.enemyCanMove = true;
        }
    }
    
    private void DealDamage(EnemyHealthPoints enemyHealthPoints)
    {
        var _bulletPenetrationCount = Player.Instance.maxPenetrationCount;
        
        if (!Player.Instance.endlessPenetrationBullets)
        {
            _bulletPenetrationCount -= 1;
            enemyHealthPoints.TakeDamage(Player.Instance.bulletDamage);
        }
        else
        {
            enemyHealthPoints.TakeDamage(criticalDamage);
        }
        
        if (_bulletPenetrationCount >= 0) 
            return;
        
        if (!Player.Instance.stickyBullets)
        {
            DeactivateBullet();
        }
    }

    private void DeactivateBullet()
    {
        if (!Player.Instance.stickyBullets)
        {
            var _bulletImpactParticles = BulletPoolingManager.Instance.impactParticles;
            _bulletImpactParticles.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);

            _bulletImpactParticles.Play();
        }

        if (popCornParticle)
        {
            var _popCornParticles = BulletPoolingManager.Instance.popcornParticles;
            _popCornParticles.transform.position = gameObject.transform.position;
            _popCornParticles.Play();
            Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(transform.position, 3);

            foreach (var _enemy in _hitEnemies)
            {
                if (_enemy.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints))
                {
                    _enemyHealthPoints.TakeDamage(explosiveDamage);
                }
            }
        }
        
        gameObject.SetActive(false);
    }
}
