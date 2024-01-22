using UnityEngine;

public class EnemyHealthPoints : MonoBehaviour
{
    [SerializeField] private int maximumHitPoints = 5;
    [SerializeField] private float currentHitPoints;

    private void Start()
    {
        currentHitPoints = maximumHitPoints;
    }

    //Here I subtract the health from the damage amount that is given with every call of the method, then I play a hit stop
    public void TakeDamage(float damageAmount)
    {
        currentHitPoints -= damageAmount;
        
        gameObject.GetComponent<Enemy>().Stop(gameObject.GetComponent<Enemy>().changeColorTime);

        if(currentHitPoints <= 0)
            Destroy(gameObject);
    }
}
