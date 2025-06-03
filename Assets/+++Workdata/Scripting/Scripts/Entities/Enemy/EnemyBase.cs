using System;
using System.Collections;
using System.Linq;
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
    [HideInInspector] public Transform target;

    [Header("Particles")]
    [SerializeField] private GameObject enemyDeathMark;
    [SerializeField] private GameObject enemyConfetti;
    [FormerlySerializedAs("enemyShot")] public ParticleSystem enemyShotConfetti;
    
    [Header("Ride")]
    [SerializeField] private float rideAttackDamage = 1;
    private bool gotKilledFromRide;

    [FormerlySerializedAs("addCurrencyOnDeath")]
    [Header("Drops")] 
    [HideInInspector] public bool addHelpDropsOnDeath = true;
    [SerializeField] private GameObject ammoDropPrefab;
    [SerializeField] private GameObject currencyDropPrefab;
    [FormerlySerializedAs("enemyAbilityGain")] [SerializeField] private float enemyAbilityGainForPlayer = 1;
    [SerializeField] private Vector2Int ammunitionAmountDropRange;
    [Range(0,1), SerializeField] private float ammunitionDropChancePercentage;
    [SerializeField] private Vector2 currencyDropRange;

    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        target = Ride.Instance.transform;
    }

    private void Update()
    {
        EnemyFreezeUpdate();

        ChangeTargetOnRange();
    }

    private void ChangeTargetOnRange()
    {
        if (Vector2.Distance(transform.position, Ride.Instance.transform.position) <
            Vector2.Distance(transform.position, PlayerBehaviour.Instance.transform.position))
        {
            target = Ride.Instance.transform;
        }
    }

    private void EnemyFreezeUpdate()
    {
        enemyCanMove = EnemyFreezeTime <= 0;

        if(EnemyFreezeTime > 0)
            EnemyFreezeTime -= Time.deltaTime;
    }
    
    public IEnumerator HitVisual()
    {
        AudioManager.Instance.Play("EnemyHit");
        sr.color = Color.red;

        yield return new WaitForSeconds(.1f);
        
        sr.color = Color.white;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out Ride _ride))
        {
            _ride.currentRideHealth -= rideAttackDamage;
            Ride.Instance.rideHealthFill.fillAmount = _ride.currentRideHealth / _ride.maxRideHealth;

            if (_ride.currentRideHealth <= 0)
            {
                _ride.LostWave();
            }
            
            Ride.Instance.StartRideHitVisual();
            AudioManager.Instance.Play("RideHit");
            
            gotKilledFromRide = true;
            Destroy(gameObject);
        }
        else if (other.gameObject.TryGetComponent(out PlayerBehaviour _playerBehaviour) && target == _playerBehaviour.transform)
        {
            _playerBehaviour.StartHitVisual();
            
            gotKilledFromRide = true;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Ride.Instance.canWinGame)
        {
            int _enemies = Ride.Instance.enemyParent.transform.Cast<Transform>().Count(child => child.GetComponent<EnemyBase>());

            Debug.Log("Tried To Win Game");

            if (_enemies <= 1)
            {
                Ride.Instance.WonWave();
            }
        }
        
        if(!gameObject.scene.isLoaded || gotKilledFromRide) 
            return;

        var _transform = transform;
        Instantiate(enemyDeathMark, _transform.position, Quaternion.identity, _transform.parent);
            
        var _confetti = Instantiate(enemyConfetti, _transform.position, Quaternion.identity, _transform.parent);
        _confetti.GetComponent<ParticleSystem>().Play();

        if (addHelpDropsOnDeath)
        {
            PlayerBehaviour.Instance.abilityBehaviour.AddAbilityFill(enemyAbilityGainForPlayer);
            GameObject _currencyDrop = Instantiate(currencyDropPrefab, transform.position, Quaternion.identity);
            _currencyDrop.GetComponent<CurrencyDrop>().currencyCount = Random.Range((int)currencyDropRange.x, (int)currencyDropRange.y);

            if (Random.value <= ammunitionDropChancePercentage)
            {
                int _ammoAmount = Random.Range(ammunitionAmountDropRange.x, ammunitionAmountDropRange.y + 1);
                GameObject _ammoDrop = Instantiate(ammoDropPrefab, transform.position, Quaternion.identity);
                _ammoDrop.GetComponent<AmmoDrop>().ammoCount = _ammoAmount;
            }
        }
    }
}
