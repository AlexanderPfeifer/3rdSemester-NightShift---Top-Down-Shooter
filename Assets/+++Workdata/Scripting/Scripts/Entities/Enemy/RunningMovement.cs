using UnityEngine;

public class RunningMovement : MonoBehaviour
{
    [Header("Movement")]
    private EnemyBase enemyBase;

    private void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    private void FixedUpdate()
    {
        MoveToRide();
    }

    private void MoveToRide()
    {
        if(!enemyBase.enemyCanMove)
            return;

        enemyBase.rbEnemy.MovePosition(transform.position =  Vector2.MoveTowards(transform.position, enemyBase.target.position, enemyBase.currentSpeed * Time.deltaTime));

        enemyBase.sr.flipX = !(transform.InverseTransformPoint(enemyBase.target.position).x > 0);
    }
}
