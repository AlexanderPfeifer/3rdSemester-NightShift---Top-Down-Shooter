using UnityEngine;

public class JumpingMovement : MonoBehaviour
{
    [Header("Movement")]
    private bool bunnyCanJump;
    [SerializeField] private float bunnyJumpSpeed;
    private EnemyBase enemyBase;

    private void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    private void FixedUpdate()
    {
        if(!enemyBase.enemyCanMove)
            return;


        if (bunnyCanJump)
        {
            //I set bunnyCanJump on true in an animation because the movement of the bunny is in jumps, not a consistent running
            enemyBase.rbEnemy.MovePosition(transform.position = Vector2.MoveTowards(transform.position, enemyBase.target.position, bunnyJumpSpeed * Time.deltaTime));
        }
        
        enemyBase.sr.flipX = !(transform.InverseTransformPoint(enemyBase.target.position).x > 0);
    }
    
    //Animation event
    public void BunnyCanJump()
    {
        bunnyCanJump = !bunnyCanJump;
    }
}
