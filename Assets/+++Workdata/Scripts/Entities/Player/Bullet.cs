using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    private const float BulletDistanceUntilDestroy = 25;
    private Player player;

    [SerializeField] private LayerMask enemyLayer;

    private Vector2 startPosition;

    [SerializeField] private float tickStickyBullet;
    [SerializeField] private float maxTickStickyBullet = 2;
    
    private bool canGetAbilityGain;

    private void Update()
    {
        if (player.abilityProgress <= 0)
        {
            canGetAbilityGain = true;
        }

        if (player.stickyBullets)
        {
            tickStickyBullet -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (Vector2.Distance(startPosition, transform.position) >= BulletDistanceUntilDestroy)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Player shooter, Vector2 targetedPosition)
    {
        player = FindObjectOfType<Player>();

        var direction = targetedPosition - (Vector2)shooter.weaponEndPoint.transform.position;
        direction.Normalize();

        var bulletTransform = transform;
        bulletTransform.up = direction;
        
        startPosition = bulletTransform.position;
        
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooter.GetComponent<Collider2D>());
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GetComponent<Collider2D>());
        
        GetComponent<Rigidbody2D>().AddForce(direction * player.bulletSpeed, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != enemyLayer)
        {
            Destroy(gameObject);
        }
        
        var healthPointManager = col.gameObject.GetComponent<EnemyHealthPoints>();
        if (healthPointManager != null)
        {
            if (player.freezeBullets)
            {
                canGetAbilityGain = false;

                col.gameObject.GetComponent<Enemy>().EnemyFreeze();    
            }

            if (player.explosiveBullets)
            {
                canGetAbilityGain = false;

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 4);
                
                foreach (var enemy in hitEnemies)
                {
                    if (enemy.GetComponent<EnemyHealthPoints>())
                    {
                        enemy.GetComponent<EnemyHealthPoints>().TakeDamage(player.bulletDamage);
                    }
                }
            }

            if (player.endlessPenetrationBullets)
            {
                canGetAbilityGain = false;
            }

            if (player.splitBullets)
            {
                canGetAbilityGain = false;
                
                Vector2 bulletDirection = Random.insideUnitCircle;
                bulletDirection.Normalize();
                
                var newBullet = Instantiate(player.bulletPrefab, transform.position, Quaternion.identity);
            
                newBullet.GetComponent<Rigidbody2D>().AddForce(bulletDirection * player.bulletSpeed, ForceMode2D.Impulse);
            }

            if (player.stickyBullets)
            {
                canGetAbilityGain = false;
                transform.position = col.gameObject.transform.position;
                
                if (tickStickyBullet <= 0)
                {
                    col.gameObject.GetComponent<EnemyHealthPoints>().TakeDamage(player.bulletDamage);
                    tickStickyBullet = maxTickStickyBullet;
                }
            }
            
            BulletBehaviour(healthPointManager);
        }
    }

    private void BulletBehaviour(EnemyHealthPoints enemyHealthPoints)
    {
        var bulletPenetrationCount = player.maxPenetrationCount;
        if (!player.endlessPenetrationBullets)
        {
            bulletPenetrationCount -= 1;
        }
        enemyHealthPoints.TakeDamage(player.bulletDamage);
        if (canGetAbilityGain)
        {
            player.abilityProgress += player.activeAbilityGain;
        }
        
        if (bulletPenetrationCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
