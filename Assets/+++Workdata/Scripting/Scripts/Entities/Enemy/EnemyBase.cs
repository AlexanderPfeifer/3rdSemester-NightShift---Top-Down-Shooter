using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    [FormerlySerializedAs("maxEnemyKnockBackResistance")] public float knockBackResistance = 10;
    [HideInInspector] public bool enemyCanMove = true;
    [HideInInspector] public float enemyFreezeTime;
    [HideInInspector] public Rigidbody2D rbEnemy;

    [Header("Health")]
    [SerializeField] private GameObject enemyDeathMark;
    [SerializeField] private GameObject enemyConfetti;
    [HideInInspector] public SpriteRenderer sr;
    
    [Header("Ride")]
    [SerializeField] private float rideAttackDamage = 1;
    private bool gotKilledFromRide;
    [HideInInspector] public Ride ride;
    
    [Header("Drops")]
    [FormerlySerializedAs("enemyAbilityGain")] [SerializeField] private float enemyAbilityGainForPlayer = 1;

    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();
        ride = GetComponentInParent<Ride>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        EnemyFreezeUpdate();
    }

    private void EnemyFreezeUpdate()
    {
        enemyCanMove = enemyFreezeTime <= 0;

        if(enemyFreezeTime > 0)
            enemyFreezeTime -= Time.deltaTime;
    }
    
    public void HitVisual()
    {
        GetComponent<Animator>().SetTrigger("Hurt");
        
        AudioManager.Instance.Play("EnemyHit");

        if (Player.Instance.explosiveBullets)
        {
            AudioManager.Instance.Play("PopCornExplosion");
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.TryGetComponent(out Ride _ride))
        {
            _ride.currentRideHealth -= rideAttackDamage;
            ride.StartRideHitVisual();
            gotKilledFromRide = true;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (!gotKilledFromRide)
        {
            var _transform = transform;
            Instantiate(enemyDeathMark, _transform.position, Quaternion.identity, _transform.parent);
            
            var _confetti = Instantiate(enemyConfetti, _transform.position, Quaternion.identity, _transform.parent);
            _confetti.GetComponent<ParticleSystem>().Play();
            
            if (Player.Instance.canGetAbilityGain)
            {
                var _player = Player.Instance;
                _player.currentAbilityTime += enemyAbilityGainForPlayer;
                InGameUIManager.Instance.abilityProgress.GetComponent<Slider>().value = _player.currentAbilityTime / _player.maxAbilityTime;

                if (_player.currentAbilityTime >= _player.maxAbilityTime)
                {
                    InGameUIManager.Instance.pressSpace.SetActive(true);
                }
            }
        }
    }
}
