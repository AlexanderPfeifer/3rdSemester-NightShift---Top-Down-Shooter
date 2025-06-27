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
    [HideInInspector] public Rigidbody2D rbEnemy;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public Transform target;
    [NonSerialized] public float currentSpeed;
    [SerializeField] private float speedOnRide;
    [HideInInspector] public float speedOnHit;

    [Header("Particles")]
    [SerializeField] private GameObject enemyDeathMark;
    [SerializeField] private GameObject enemyConfetti;
    [FormerlySerializedAs("enemyShot")] public ParticleSystem enemyShotConfetti;
    
    [Header("Ride")]
    [SerializeField] private float rideAttackDamage = 1;
    private bool gotKilledFromRide;
    [SerializeField] private float screenShakeStrength = 8;

    [FormerlySerializedAs("addCurrencyOnDeath")]
    [Header("Drops")] 
    [HideInInspector] public bool addHelpDropsOnDeath = true;
    [SerializeField] private GameObject ammoDropPrefab;
    [SerializeField] private GameObject currencyDropPrefab;
    [FormerlySerializedAs("enemyAbilityGain")] [SerializeField] private float enemyAbilityGainForPlayer = 1;
    [SerializeField] private Vector2Int ammunitionAmountDropRange;
    [Range(0,1), SerializeField] private float ammunitionDropChancePercentage;
    [SerializeField] private Vector2 currencyDropRange;

    [Header("Health")]
    [SerializeField] private Material hitWhiteMaterial;
    [SerializeField] private Material standartMaterial;

    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        target = Ride.Instance.transform;
        currentSpeed = speedOnRide;
        speedOnHit = speedOnRide * 2;
    }

    private void Update()
    {
        ChangeTargetOnRange();
    }

    private void ChangeTargetOnRange()
    {
        if (Vector2.Distance(transform.position, Ride.Instance.transform.position) <
            Vector2.Distance(transform.position, PlayerBehaviour.Instance.transform.position))
        {
            target = Ride.Instance.transform;
            currentSpeed = speedOnRide;
        }
    }

    public IEnumerator EnemyFreezeCoroutine(float freezeTime)
    {
        enemyCanMove = false;
        sr.color = Color.blue;

        yield return new WaitForSecondsRealtime(freezeTime);

        sr.color = Color.white;
        enemyCanMove = true;
    }

    public IEnumerator HitVisual()
    {
        AudioManager.Instance.Play("EnemyHit");
        sr.material = hitWhiteMaterial;

        yield return new WaitForSeconds(0.05f);
        
        sr.material = standartMaterial;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out Ride _ride))
        {
            Ride.Instance.ReceiveDamage(rideAttackDamage, screenShakeStrength);
                        
            gotKilledFromRide = true;
            Destroy(gameObject);
        }
        else if (other.gameObject.TryGetComponent(out PlayerBehaviour _playerBehaviour) && target == _playerBehaviour.transform)
        {
            _playerBehaviour.StartHitVisual();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (Ride.Instance.canWinGame)
        {
            int _enemies = Ride.Instance.enemyParent.transform.Cast<Transform>().Count(child => child.GetComponent<EnemyBase>());

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
            GameObject _currencyDrop = Instantiate(currencyDropPrefab, transform.position, Quaternion.identity, _transform.parent);
            _currencyDrop.GetComponent<CurrencyDrop>().currencyCount = Random.Range((int)currencyDropRange.x, (int)currencyDropRange.y);

            if (!GetComponent<EnemyHealthPoints>().followsPlayerOnBeingShot)
            {
                _currencyDrop.transform.localScale = new Vector3(_currencyDrop.transform.localScale.x * 2, _currencyDrop.transform.localScale.y * 2, _currencyDrop.transform.localScale.z);
            }

            if (Random.value <= ammunitionDropChancePercentage)
            {
                int _ammoAmount = Random.Range(ammunitionAmountDropRange.x, ammunitionAmountDropRange.y + 1);
                
                GameObject _ammoDrop = Instantiate(ammoDropPrefab, transform.position, Quaternion.identity, _transform.parent);
                _ammoDrop.GetComponent<AmmoDrop>().ammoCount = _ammoAmount;

                if (GetComponent<EnemyHealthPoints>().followsPlayerOnBeingShot)
                {
                    _ammoDrop.transform.localScale = new Vector3(_ammoDrop.transform.localScale.x / 2, _ammoDrop.transform.localScale.y / 2, _ammoDrop.transform.localScale.z);
                }
            }
        }
    }
}
