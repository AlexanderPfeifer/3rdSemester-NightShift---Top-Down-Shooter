using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 14;
    [SerializeField] private float bulletDistanceUntilDestroy = 10;
    [SerializeField] private int bulletDamage = 1;
    private Player player;
    
    private Vector2 startPosition;

    private void FixedUpdate()
    {
        if (Vector2.Distance(startPosition, transform.position) >= bulletDistanceUntilDestroy)
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
        var healthPointManager = col.gameObject.GetComponent<HealthPointManager>();
        if (healthPointManager != null)
        {
            healthPointManager.TakeDamage(bulletDamage);
        }
        
        Destroy(gameObject);
    }
}
