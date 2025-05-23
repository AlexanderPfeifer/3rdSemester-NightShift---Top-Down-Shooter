using UnityEngine;

public class ZigZagMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zigzagAmplitude = 1f;
    [SerializeField] private float zigzagFrequency = 2f;
    private float zigzagTimer;
    
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
        if (!enemyBase.enemyCanMove)
            return;

        var _ridePosition = Ride.Instance.transform.position;
        var _position = transform.position;
        zigzagTimer += Time.deltaTime * zigzagFrequency;
        
        float _scaledZigZagAmplitude = Mathf.Min(zigzagAmplitude, Vector2.Distance(_position, _ridePosition));
        float _offsetAmount = Mathf.Sin(zigzagTimer) * _scaledZigZagAmplitude;
        
        var _direction = (_ridePosition - _position).normalized;
        Vector2 _zigzagOffset = new Vector2(-_direction.y, _direction.x) * _offsetAmount;
        
        Vector2 _zigzagTarget = _ridePosition + (Vector3)_zigzagOffset;
        Vector2 _newPos = Vector2.MoveTowards(_position, _zigzagTarget, moveSpeed * Time.deltaTime);
        enemyBase.rbEnemy.MovePosition(_position = _newPos);
        transform.position = _position;

        enemyBase.sr.flipX = !(transform.InverseTransformPoint(_ridePosition).x > 0);
    }
}
