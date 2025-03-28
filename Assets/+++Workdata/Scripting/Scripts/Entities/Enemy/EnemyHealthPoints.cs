using UnityEngine;

public class EnemyHealthPoints : MonoBehaviour
{
    [SerializeField] private int maximumHitPoints = 5;
    private float currentHitPoints;

    private void Start()
    {
        currentHitPoints = maximumHitPoints;
    }

    public void TakeDamage(float damageAmount, Transform bulletTransform)
    {
        currentHitPoints -= damageAmount;
        
        var _enemy = gameObject.GetComponent<EnemyBase>();
        _enemy.HitVisual();

        if (currentHitPoints <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (bulletTransform != null)
        {
            var _enemyShotParticles = Instantiate(_enemy.enemyShotConfetti, transform.position, Quaternion.identity, _enemy.ride.enemyParent.transform);
            _enemyShotParticles.transform.localRotation = bulletTransform.localRotation;
            _enemyShotParticles.Play();
        }
    }
}
