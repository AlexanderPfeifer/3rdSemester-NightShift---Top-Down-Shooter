using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    private const float BulletDistanceUntilDestroy = 25;
    private Player player;
    private Vector2 startPosition;

    [SerializeField] private float tickStickyBullet;
    [SerializeField] private float maxTickStickyBullet = 2;
    
    [SerializeField] private float explosiveDamage = 3;

    [SerializeField] private float tickDamage;

    private float currentBulletDamage;

    private bool bulletGotSplit;

    private Vector2 travelDirection;

    private Color noAlpha;

    private void OnEnable()
    {
        player = FindObjectOfType<Player>();
        currentBulletDamage = player.bulletDamage;
    }
    
    private void Update()
    {
        if (player.stickyBullets)
        {
            tickStickyBullet -= Time.deltaTime;

            if (tickStickyBullet <= 0)
            {
                var enemy = GetComponentInParent<Enemy>();
                tickStickyBullet = maxTickStickyBullet;
                enemy.GetComponent<EnemyHealthPoints>().TakeDamage(tickDamage);
                enemy.Stop(enemy.changeColorTime);
            }
        }
    }

    private void FixedUpdate()
    {
        if (Vector2.Distance(startPosition, transform.position) >= BulletDistanceUntilDestroy)
        {
            Destroy(gameObject);
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
        if (!col.gameObject.GetComponent<Enemy>())
        {
            Destroy(gameObject);
        }
        
        var healthPointManager = col.gameObject.GetComponent<EnemyHealthPoints>();
        
        var enemyObject = col.gameObject.GetComponent<Enemy>();

        if (healthPointManager != null)
        {
            if (player.freezeBullets)
            {
                col.gameObject.GetComponent<Enemy>().EnemyFreeze();
            }

            if (player.endlessPenetrationBullets)
            {
                var probability = Random.Range(1, 10);
                if (probability == 10)
                {
                    currentBulletDamage = Random.Range(10, 20);
                }
            }

            if (player.explosiveBullets)
            {
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 3);
                
                foreach (var enemy in hitEnemies)
                {
                    if (enemy.GetComponent<EnemyHealthPoints>())
                    {
                        enemy.GetComponent<EnemyHealthPoints>().TakeDamage(explosiveDamage);
                        enemy.GetComponent<Enemy>().Stop(enemyObject.changeColorTime);
                    }
                }
            }
            
            if (player.splitBullets && !bulletGotSplit)
            {
                Vector2 bulletDirection = Random.insideUnitCircle;
                bulletDirection.Normalize();
                
                var newBullet = Instantiate(player.bulletPrefab, transform.position, Quaternion.identity);
            
                newBullet.GetComponent<Rigidbody2D>().AddForce(bulletDirection * player.bulletSpeed, ForceMode2D.Impulse);

                newBullet.bulletGotSplit = true;

                bulletGotSplit = true;
            }

            if (player.stickyBullets)
            {
                transform.SetParent(col.gameObject.transform);
            }
            
            if(player.explosiveBullets)
                return;

            enemyObject.Stop(enemyObject.changeColorTime);

            if (bulletGotSplit)
            {
                healthPointManager.TakeDamage(player.bulletDamage);
                Destroy(gameObject);
            }

            enemyObject.StartCoroutine(EnemyKnockBack(enemyObject));
            
            BulletBehaviour(healthPointManager);
        }
    }
    
    private IEnumerator EnemyKnockBack(Enemy enemy)
    {
        enemy.enemyCanMove = false;
        
        enemy.GetComponent<Rigidbody2D>().AddForce(travelDirection * enemy.enemyKnockBack, ForceMode2D.Impulse);

        yield return new WaitForSecondsRealtime(0.15f);

        enemy.enemyCanMove = true;
    }
    
    private void BulletBehaviour(EnemyHealthPoints enemyHealthPoints)
    {
        var bulletPenetrationCount = player.maxPenetrationCount;
        
        if (!player.endlessPenetrationBullets)
        {
            bulletPenetrationCount -= 1;
        }
        
        enemyHealthPoints.TakeDamage(player.bulletDamage);

        if (bulletPenetrationCount < 0)
        {
            if (!player.stickyBullets)
            {
                Destroy(gameObject);
            }
        }
    }
}
