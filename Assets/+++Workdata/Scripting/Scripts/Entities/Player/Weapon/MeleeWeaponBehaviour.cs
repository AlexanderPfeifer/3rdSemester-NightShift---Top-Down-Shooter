using UnityEngine;

public class MeleeWeaponBehaviour : MonoBehaviour
{
    private BoxCollider2D hitCollider;
    [SerializeField] private float damage;

    private void Start()
    {
        hitCollider = GetComponent<BoxCollider2D>();
    }

    public void SetActiveHitCollider()
    {
        hitCollider.enabled = !hitCollider.enabled;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints))
        {
            _enemyHealthPoints.TakeDamage(damage, null);
            _enemyHealthPoints.StartCoroutine(_enemyHealthPoints.EnemyKnockBack(0, _enemyHealthPoints.transform.position - transform.position));
        }
    }
}
