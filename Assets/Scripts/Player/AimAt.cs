using UnityEngine;
using UnityEngine.InputSystem;

public class AimAt : MonoBehaviour
{
    [SerializeField] private float speed = 20;
    private Camera mainCamera;
    
    private Vector2 lerpMouse;

    private void Start()
    {
        mainCamera = FindObjectOfType<Camera>(); 
    }

    private void Update()
    {
        GetMousePosition();
    }

    public Vector2 GetMousePosition()
    {
        lerpMouse = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        lerpMouse = new Vector2(Mathf.Round(lerpMouse.x * 10.0f) * 0.1f, Mathf.Round(lerpMouse.y * 10.0f) * 0.1f);
        var position = transform.position;
        position = Vector2.Lerp(position, lerpMouse, speed * Time.deltaTime);
        transform.position = position;

        return lerpMouse;
    }
}
