using System.Collections;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rbEnemy;
    [SerializeField] private GameObject enemyDeathMark;
    private SpriteRenderer sr;
    private Ride ride;

    [Header("Booleans")]
    [SerializeField] private bool bigEnemy;
    [SerializeField] private bool isBunny;
    private bool bunnyCanJump;
    public bool enemyCanMove = true;
    public bool enemyWaiting;
    private bool rideDeath;
    private bool enemyFreeze;

    [Header("Floats")]
    [SerializeField] private float otherEnemiesMoveSpeed;
    [SerializeField] private float rideAttackDamage = 1;
    public float changeColorTime = .03f;
    public float currentEnemyKnockBackResistance;
    public float currentEnemyKnockBackResistanceDoubled;
    public float maxEnemyKnockBackResistance = 10;
    [SerializeField] private float bunnyJumpSpeed;
    [SerializeField] public float enemyFreezeTime = 5f;
    [SerializeField] private float enemyAbilityGain = 1;
    
    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();
        
        ride = GetComponentInParent<Ride>();

        sr = GetComponent<SpriteRenderer>();

        currentEnemyKnockBackResistance = maxEnemyKnockBackResistance;

        currentEnemyKnockBackResistanceDoubled *= 2;
    }

    private void Update()
    {
        EnemyFreeze();
    }

    private void FixedUpdate()
    {
        if (enemyCanMove)
        {
            currentEnemyKnockBackResistance = maxEnemyKnockBackResistance;
            MoveToRide();
        }
        else
        {
            currentEnemyKnockBackResistance = currentEnemyKnockBackResistanceDoubled;
        }
    }

    private void MoveToRide()
    {
        //I set bunnyCanJump on true in an animation because the movement of the bunny is in jumps, not a consistent running
        if (!isBunny)
        {
            rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, ride.transform.position, otherEnemiesMoveSpeed * Time.deltaTime));
        }
        else
        {
            if (bunnyCanJump)
            {
                rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, ride.transform.position, bunnyJumpSpeed * Time.deltaTime));
            }
        }
        
        sr.flipX = !(transform.InverseTransformPoint(ride.transform.position).x > 0);
    }

    private void EnemyFreeze()
    {
        if (!enemyFreeze)
        {
            return;
        }
        
        if (enemyFreezeTime <= 0)
        {
            enemyCanMove = true;
            enemyFreeze = false;
        }
        else
        {
            enemyCanMove = false;
            enemyFreeze = true;
        }

        enemyFreezeTime -= Time.deltaTime;
    }

    public void HitStop(float duration)
    {
        if (enemyWaiting)
        {
            return;
        }
        
        GetComponent<Animator>().SetTrigger("Hurt");
        
        StartCoroutine(EnemyGotHit(duration));
    }
    
    private IEnumerator EnemyGotHit(float duration)
    {
        enemyWaiting = true;

        yield return new WaitForSecondsRealtime(duration);
        
        AudioManager.Instance.Play("EnemyHit");

        if (Player.Instance.explosiveBullets)
        {
            AudioManager.Instance.Play("PopCornExplosion");
        }
        
        enemyWaiting = false;
    }

    //Animation event
    public void BunnyCanJump()
    {
        bunnyCanJump = !bunnyCanJump;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<Ride>())
        {
            col.gameObject.GetComponent<Ride>().currentRideHealth -= rideAttackDamage;
            ride.StartRideHitVisual(changeColorTime);
            Destroy(gameObject);
            rideDeath = true;
        }
    }

    //So the time doesnt freeze completely when the enemy dies, I switch it back to 1
    private void OnDestroy()
    {
        Time.timeScale = 1;

        if (!rideDeath)
        {
            var _deathMark = Instantiate(enemyDeathMark, transform.position, Quaternion.identity, Player.Instance.bulletParent.transform);
            
            if (bigEnemy)
            {
                _deathMark.transform.localScale = new Vector3(2f, 2f, 2f);
                ride.gameObject.GetComponentInChildren<ParticleSystem>().transform.position = transform.position;
                ride.gameObject.GetComponentInChildren<ParticleSystem>().transform.localScale = new Vector3(1f, 1f, 1f);
                ride.gameObject.GetComponentInChildren<ParticleSystem>().Play();
            }
            else
            {
                ride.gameObject.GetComponentInChildren<ParticleSystem>().transform.position = transform.position;
                ride.gameObject.GetComponentInChildren<ParticleSystem>().transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                ride.gameObject.GetComponentInChildren<ParticleSystem>().Play();
            }
        }

        if (Player.Instance.canGetAbilityGain && !rideDeath)
        {
            Player.Instance.currentAbilityTime += enemyAbilityGain;

            if (Player.Instance.currentAbilityTime >= Player.Instance.maxAbilityTime)
            {
                InGameUIManager.Instance.pressSpace.SetActive(true);
            }
        }
    }
}
