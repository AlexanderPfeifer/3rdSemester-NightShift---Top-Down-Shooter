using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    [FormerlySerializedAs("maxEnemyKnockBackResistance")] public float knockBackResistance = 10;
    [HideInInspector] public bool enemyCanMove = true;
    [NonSerialized] public float EnemyFreezeTime;
    [HideInInspector] public Rigidbody2D rbEnemy;
    [HideInInspector] public SpriteRenderer sr;

    [Header("Particles")]
    [SerializeField] private GameObject enemyDeathMark;
    [SerializeField] private GameObject enemyConfetti;
    [FormerlySerializedAs("enemyShot")] public ParticleSystem enemyShotConfetti;
    
    [Header("Ride")]
    [SerializeField] private float rideAttackDamage = 1;
    private bool gotKilledFromRide;
    [HideInInspector] public Ride ride;
    
    [Header("Drops")]
    [SerializeField] private GameObject ammoDropPrefab;
    [FormerlySerializedAs("enemyAbilityGain")] [SerializeField] private float enemyAbilityGainForPlayer = 1;
    [SerializeField] private Vector2Int ammunitionAmountDropRange;
    [Range(0,1), SerializeField] private float ammunitionDropChancePercentage;
    [SerializeField] private Vector2 currencyDropRange;

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
        enemyCanMove = EnemyFreezeTime <= 0;

        if(EnemyFreezeTime > 0)
            EnemyFreezeTime -= Time.deltaTime;
    }
    
    public void HitVisual()
    {
        GetComponent<Animator>().SetTrigger("Hurt");
        
        AudioManager.Instance.Play("EnemyHit");
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
        if(!gameObject.scene.isLoaded || gotKilledFromRide) 
            return;
        
        var _transform = transform;
        Instantiate(enemyDeathMark, _transform.position, Quaternion.identity, _transform.parent);
            
        var _confetti = Instantiate(enemyConfetti, _transform.position, Quaternion.identity, _transform.parent);
        _confetti.GetComponent<ParticleSystem>().Play();
        
        PlayerBehaviour.Instance.abilityBehaviour.AddAbilityFill(enemyAbilityGainForPlayer);

        if (Random.value <= ammunitionDropChancePercentage)
        {
            int _ammoAmount = Random.Range(ammunitionAmountDropRange.x, ammunitionAmountDropRange.y + 1);
            GameObject _ammoDrop = Instantiate(ammoDropPrefab, transform.position, Quaternion.identity);
            _ammoDrop.GetComponent<AmmoDrop>().ammoCount = _ammoAmount;
        }
        
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(Random.Range((int)currencyDropRange.x, (int)currencyDropRange.y));
    }
}
