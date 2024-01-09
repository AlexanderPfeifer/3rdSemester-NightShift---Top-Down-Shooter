using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private int penetrationCount;
    [SerializeField] private float fireRate;
    [SerializeField] private float activeAbilityGain;
    [SerializeField] private float bulletSpeed = 38;

    private const float BulletDistanceUntilDestroy = 25;
    private Player player;
    
    private Vector2 startPosition;

    private void FixedUpdate()
    {
        if (Vector2.Distance(startPosition, transform.position) >= BulletDistanceUntilDestroy)
        {
            Destroy(gameObject);
        }
    }
    
    public void Launch(Player shooter, Vector2 targetedPosition)
    {
        var direction = targetedPosition - (Vector2)shooter.weaponEndPoint.transform.position;
        direction.Normalize();

        var bulletTransform = transform;
        bulletTransform.up = direction;
        
        startPosition = bulletTransform.position;
        
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooter.GetComponent<Collider2D>());
        
        GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        var healthPointManager = col.gameObject.GetComponent<EnemyHealthPoints>();
        if (healthPointManager != null)
        {
            healthPointManager.TakeDamage(bulletDamage);
        }
        
        Destroy(gameObject);
    }
}
