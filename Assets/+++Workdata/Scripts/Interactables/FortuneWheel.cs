using UnityEngine;

public class FortuneWheel : MonoBehaviour
{
    private GameInputManager gameInputManager;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<Player>(out var player))
        {
            
        }
    }
}
