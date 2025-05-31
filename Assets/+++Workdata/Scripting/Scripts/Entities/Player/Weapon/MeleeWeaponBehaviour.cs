using UnityEngine;

public class MeleeWeaponBehaviour : MonoBehaviour
{
    [HideInInspector] public CapsuleCollider2D hitCollider;
    [SerializeField] private float damage;
    public float knockBack = 5;

    private void Start()
    {
        hitCollider = GetComponent<CapsuleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints))
        {
            _enemyHealthPoints.TakeDamage(damage, null);
            _enemyHealthPoints.StartCoroutine(_enemyHealthPoints.EnemyKnockBack(0, _enemyHealthPoints.transform.position - transform.position));
        }
        else if (col.TryGetComponent(out ShootingSignBehaviour _shootingSignBehaviour))
        {
            if (_shootingSignBehaviour.canGetHit)
            {
                _shootingSignBehaviour.StartCoroutine(_shootingSignBehaviour.SnapDownOnHit(false));
            }
        }
    }
}
