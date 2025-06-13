using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class MeleeWeaponBehaviour : MonoBehaviour
{
    [HideInInspector] public CapsuleCollider2D hitCollider;
    [SerializeField] private float damage;
    public float knockBack = 5;
    [SerializeField] private float onHitScreenShakeValue = 3.5f;

    private void Start()
    {
        hitCollider = GetComponent<CapsuleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.TryGetComponent(out EnemyHealthPoints _enemyHealthPoints))
        {
            WeaponEffects();    
            _enemyHealthPoints.TakeDamage(damage, null);
            _enemyHealthPoints.StartCoroutine(_enemyHealthPoints.EnemyKnockBack(0, _enemyHealthPoints.transform.position - transform.position));
        }
        else if (col.TryGetComponent(out ShootingSignBehaviour _shootingSignBehaviour))
        {
            if (_shootingSignBehaviour.canGetHit)
            {
                WeaponEffects();
                _shootingSignBehaviour.StartCoroutine(_shootingSignBehaviour.SnapDownOnHit());
            }
        }
    }

    private void WeaponEffects()
    {
        AudioManager.Instance.Play("BatonHit");
        StartCoroutine(WeaponVisualCoroutine());
    }
    
    public IEnumerator WeaponVisualCoroutine()
    {
        PlayerBehaviour.Instance.weaponBehaviour.playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = onHitScreenShakeValue;
        
        //AudioManager.Instance.Play("Shooting");
        
        yield return new WaitForSeconds(.1f);
        
        PlayerBehaviour.Instance.weaponBehaviour.playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = 0;
    }
}
