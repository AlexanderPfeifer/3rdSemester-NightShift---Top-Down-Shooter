using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    [Header("Components")]
    private Vector2 travelDirection;
    private Color noAlpha;
    private Rigidbody2D rb;
    private Vector2 startPosition;

    [Header("Floats")]
    private const float BulletDistanceUntilDestroy = 25;
    [SerializeField] private float tickStickyBullet;
    [SerializeField] private float maxTickStickyBullet = 2;
    [SerializeField] private float explosiveDamage = 3;
    [SerializeField] private float tickDamage;
    private float currentBulletDamage;

    [Header("Booleans")]
    private bool popCornParticle;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        currentBulletDamage = Player.Instance.bulletDamage;
    }
    
    //When sticky bullets is enabled it counts down the time an deals tick damage to the enemy it sticks to
    private void Update()
    {
        if (Player.Instance.stickyBullets)
        {
            tickStickyBullet -= Time.deltaTime;

            if (tickStickyBullet <= 0)
            {
                GetComponentInParent<EnemyHealthPoints>().TakeDamage(tickDamage);
                GetComponentInParent<Enemy>().Stop(gameObject.GetComponentInParent<Enemy>().changeColorTime);
                tickStickyBullet = maxTickStickyBullet;
            }
        }
    }

    //When bullet is too far away it destroys itself
    private void FixedUpdate()
    {
        if (Vector2.Distance(startPosition, transform.position) >= BulletDistanceUntilDestroy)
        {
            Destroy(gameObject);
        }
    }

    //Launches bullet into the direction the player shoots
    public void LaunchInDirection(Player shooter, Vector2 shootDirection)
    {
        startPosition = transform.position;
        
        GetComponent<Rigidbody2D>().AddForce(shootDirection * shooter.bulletSpeed, ForceMode2D.Impulse);

        travelDirection = shootDirection;
    }

    //The bullet always has its typical behaviour, like giving enemy knock back, hurting the enemy etc.
    //For the different abilities they all have their unique behaviour which they apply on the bullet
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.GetComponent<Enemy>())
        {
            Destroy(gameObject);
        }
        
        var healthPointManager = col.gameObject.GetComponent<EnemyHealthPoints>();
        
        var enemyObject = col.gameObject.GetComponent<Enemy>();

        if (healthPointManager == null) 
            return;
        
        if (Player.Instance.freezeBullets)
        {
            col.gameObject.GetComponent<Enemy>().enemyFreezeTime = 10f;
            col.gameObject.GetComponent<Enemy>().EnemyFreeze();
        }

        if (Player.Instance.endlessPenetrationBullets)
        {
            var probability = Random.Range(1, 10);
            if (probability == 10)
            {
                currentBulletDamage = Random.Range(10, 20);
            }
        }

        if (Player.Instance.stickyBullets)
        {
            if (col.gameObject.GetComponent<Enemy>())
            {
                transform.SetParent(col.gameObject.transform);
            }
            rb.velocity = Vector2.zero;
        }

        if (Player.Instance.explosiveBullets)
        {
            popCornParticle = true;
        }

        enemyObject.StartCoroutine(EnemyKnockBack(enemyObject));
            
        BulletBehaviour(healthPointManager);
    }
    
    //Gives enemy knock back in the direction that I set when the bullet got shot
    private IEnumerator EnemyKnockBack(Enemy enemy)
    {
        if (enemy.currentEnemyKnockBackResistance != 0)
        {
            enemy.enemyCanMove = false;
        
            enemy.GetComponent<Rigidbody2D>().AddForce(travelDirection * (Player.Instance.enemyShootingKnockBack / enemy.currentEnemyKnockBackResistance), ForceMode2D.Impulse);

            yield return new WaitForSecondsRealtime(0.15f);

            enemy.enemyCanMove = true;
        }
    }
    
    //The bullet has its penetration rate, so when penetration rate is 2, it can go through 2 enemies before being destroyed and that's what I count down here
    //And the bullets always applies damage
    private void BulletBehaviour(EnemyHealthPoints enemyHealthPoints)
    {
        var bulletPenetrationCount = Player.Instance.maxPenetrationCount;
        
        if (!Player.Instance.endlessPenetrationBullets)
        {
            bulletPenetrationCount -= 1;
        }

        enemyHealthPoints.TakeDamage(!Player.Instance.endlessPenetrationBullets
            ? Player.Instance.bulletDamage
            : currentBulletDamage);

        if (bulletPenetrationCount >= 0) 
            return;
        
        if (!Player.Instance.stickyBullets)
        {
            Destroy(gameObject);
        }
    }

    //When the bullet gets destroyed, it starts a particle effect, if the explosive bullet bool in player is true, it starts 
    //the popcorn particle
    private void OnDestroy()
    {
        var bulletImpactParticles = GetComponentInParent<Transform>().transform.GetChild(1).GetComponent<ParticleSystem>();
        bulletImpactParticles.transform.position = gameObject.transform.position;
        bulletImpactParticles.transform.rotation = gameObject.transform.rotation;
            
        bulletImpactParticles.Play();
        if (!bulletImpactParticles.isEmitting)
        {
            bulletImpactParticles.Stop();
        }

        if (!popCornParticle) 
            return;
        
        var popCornParticles = GetComponentInParent<Transform>().transform.GetChild(0).GetComponent<ParticleSystem>();
        popCornParticles.transform.position = gameObject.transform.position;
        popCornParticles.Play();
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 3);
                
        foreach (var enemy in hitEnemies)
        {
            if (enemy.GetComponent<EnemyHealthPoints>())
            {
                enemy.GetComponent<EnemyHealthPoints>().TakeDamage(explosiveDamage);
            }
        }
    }
}
