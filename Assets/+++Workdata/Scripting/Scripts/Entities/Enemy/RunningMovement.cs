using UnityEngine;

public class RunningMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private EnemyBase enemyBase;
    
    private void FixedUpdate()
    {
        MoveToRide();
    }

    private void MoveToRide()
    {
        if(!enemyBase.enemyCanMove)
            return;

        var _ridePosition = enemyBase.ride.transform.position;
        enemyBase.rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, _ridePosition, moveSpeed * Time.deltaTime));

        enemyBase.sr.flipX = !(transform.InverseTransformPoint(_ridePosition).x > 0);
    }
}
