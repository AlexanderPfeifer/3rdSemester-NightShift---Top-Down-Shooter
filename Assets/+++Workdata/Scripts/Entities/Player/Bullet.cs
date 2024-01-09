using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
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
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        var healthPointManager = col.gameObject.GetComponent<EnemyHealthPoints>();
        if (healthPointManager != null)
        {
            player.currentPenetrationCount -= 1;
            healthPointManager.TakeDamage(player.bulletDamage);
            player.abilityProgress += player.activeAbilityGain;
            
            if (player.currentPenetrationCount <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
