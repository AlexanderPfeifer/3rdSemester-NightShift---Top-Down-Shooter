using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float radius = 1;

    //This is for every spawn point of the enemies, so I can see them in the editor, i gave them this script
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
