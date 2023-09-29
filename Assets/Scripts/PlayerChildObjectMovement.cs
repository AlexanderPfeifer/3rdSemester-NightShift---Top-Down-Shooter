using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerChildObjectMovement : MonoBehaviour
{
    [HideInInspector] public Vector2 mousePosition;

    private void Update()
    {
        AimFlashlightAtMousePointer();
    }

    public void AimFlashlightAtMousePointer()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        Vector2 direction = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y);
        transform.up = direction;
    }
}
