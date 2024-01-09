using UnityEngine;

public class EnemyHealthPoints : MonoBehaviour
{
    [SerializeField] private int maximumHitPoints = 5;
    [SerializeField] private float currentHitPoints;

    private void Start()
    {
        currentHitPoints = maximumHitPoints;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHitPoints -= damageAmount;
        
        if(currentHitPoints <= 0)
            Destroy(gameObject);
    }
}
