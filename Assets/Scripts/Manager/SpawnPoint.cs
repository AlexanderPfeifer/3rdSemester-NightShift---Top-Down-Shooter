using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float radius = 1;
    
    public bool IsFreeToSpawn()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, Vector2.zero);
        if (hit.collider == null)
            return true;
        
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
