using UnityEngine;

public class Roulette : MonoBehaviour
{
    private GameInputManager gameInputManager;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<Player>(out var player))
        {
            
        }
    }
}
