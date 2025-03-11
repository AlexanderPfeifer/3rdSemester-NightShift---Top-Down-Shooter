using UnityEngine;

public class EnemyHealthPoints : MonoBehaviour
{
    [SerializeField] private int maximumHitPoints = 5;
    private float currentHitPoints;

    private void Start()
    {
        currentHitPoints = maximumHitPoints;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHitPoints -= damageAmount;
        
        var _enemy = gameObject.GetComponent<EnemyBase>();
        _enemy.HitVisual();

        if(currentHitPoints <= 0)
            Destroy(gameObject);
    }
}
