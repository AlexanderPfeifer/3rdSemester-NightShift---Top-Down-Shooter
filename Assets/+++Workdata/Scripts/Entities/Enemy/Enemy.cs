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
    [SerializeField] public float enemyFreezeTime = 10f;
    [SerializeField] private float enemyAbilityGain = 1;
    
    private void Start()
    {
        rbEnemy = GetComponent<Rigidbody2D>();
        
        ride = GetComponentInParent<Ride>();

        sr = GetComponent<SpriteRenderer>();

        currentEnemyKnockBackResistance = maxEnemyKnockBackResistance;

        currentEnemyKnockBackResistanceDoubled *= 2;
    }

    //When EnemyFreeze is called, then it sets enemyCanMove to true, so it gets called in update while enemy cannot move
    private void Update()
    {
        if(enemyFreeze)
        {
            EnemyFreeze();
        }
    }

    private void FixedUpdate()
    {
        if (enemyCanMove)
        {
            currentEnemyKnockBackResistance = maxEnemyKnockBackResistance;
            TargetRide();
        }
        else
        {
            currentEnemyKnockBackResistance = currentEnemyKnockBackResistanceDoubled;
        }
    }

    //I differentiated in isBunny and is not a bunny because the movement of the bunny is in jumps, not a consistent running
    //I set bunnyCanJump on true in an animation event of the bunny
    //Then I set the flip sprite according to the position of the ride in relation to this.
    private void TargetRide()
    {
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
        
        var relativePos = transform.InverseTransformPoint(ride.transform.position);
        
        sr.flipX = !(relativePos.x > 0);
    }

    public void EnemyFreeze()
    {
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

    //Here I stop the time for a hit stop and set the hurt animation before that, then I start a coroutine which keeps going when time is 0
    public void Stop(float duration)
    {
        if (enemyWaiting)
        {
            return;
        }
        
        GetComponent<Animator>().SetTrigger("Hurt");
        
        Time.timeScale = 0;

        StartCoroutine(EnemyGotHit(duration));
    }
    
    //Here I wait for seconds in realtime because that allows the yield to keep going when time is 0, then I set the time to 1 again
    //After that I play every sound needed, if the sounds would be played before, they would sound choppy
    private IEnumerator EnemyGotHit(float duration)
    {
        enemyWaiting = true;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        
        AudioManager.Instance.Play("EnemyHit");

        if (FindObjectOfType<Player>().explosiveBullets)
        {
            AudioManager.Instance.Play("PopCornExplosion");
        }
        
        enemyWaiting = false;
    }

    //That's for the animation event
    public void BunnyCanJump()
    {
        bunnyCanJump = !bunnyCanJump;
    }

    //If an enemy hits the ride, the ride looses and another hit stop gets played
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<Ride>())
        {
            col.gameObject.GetComponent<Ride>().currentRideHealth -= rideAttackDamage;
            ride.HitRide(changeColorTime);
            Destroy(gameObject);
            rideDeath = true;
        }
    }

    //So the time doesnt freeze completely when the enemy dies, I switch it back to 1
    //Then I only Instantiate a death mark when the player kills the enemy and make it bigger when being a big enemy
    //For the confetti particles, I make them bigger when the enemy is bigger as well
    //for the ability gain I made it per kill of enemy and again not when they run into a ride
    private void OnDestroy()
    {
        Time.timeScale = 1;

        if (!rideDeath)
        {
            var deathMark = Instantiate(enemyDeathMark, transform.position, Quaternion.identity, Player.Instance.bullets.transform);
            
            if (bigEnemy)
            {
                deathMark.transform.localScale = new Vector3(2f, 2f, 2f);
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
        }
    }
}
