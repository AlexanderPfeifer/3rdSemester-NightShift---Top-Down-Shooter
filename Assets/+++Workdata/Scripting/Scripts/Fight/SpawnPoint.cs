using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float radius = 1;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
