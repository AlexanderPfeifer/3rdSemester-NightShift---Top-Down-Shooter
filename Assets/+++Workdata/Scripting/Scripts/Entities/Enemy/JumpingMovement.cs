using UnityEngine;

public class JumpingMovement : MonoBehaviour
{
    [Header("Movement")]
    private bool bunnyCanJump;
    [SerializeField] private float bunnyJumpSpeed;
    private EnemyBase enemyBase;

    private void FixedUpdate()
    {
        if(!enemyBase.enemyCanMove)
            return;

        var _ridePosition = enemyBase.ride.transform.position;

        if (bunnyCanJump)
        {
            //I set bunnyCanJump on true in an animation because the movement of the bunny is in jumps, not a consistent running
            enemyBase.rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, enemyBase.ride.transform.position, bunnyJumpSpeed * Time.deltaTime));
        }
        
        enemyBase.sr.flipX = !(transform.InverseTransformPoint(_ridePosition).x > 0);
    }
    
    //Animation event
    public void BunnyCanJump()
    {
        bunnyCanJump = !bunnyCanJump;
    }
}
