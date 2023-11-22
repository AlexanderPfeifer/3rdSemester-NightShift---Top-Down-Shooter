using System;
using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private Player player;

    public event EventHandler OnShootingAction, OnGamePausedAction, OnInteractAction;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        
        playerInputActions.player.shoot.performed += OnPlayerShooting;
        playerInputActions.player.pause.performed += OnGamePaused;
        playerInputActions.player.interact.performed += OnPlayerInteracting;
    }
    
    private void OnPlayerShooting(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnShootingAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnGamePaused(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnGamePausedAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerInteracting(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = playerInputActions.player.move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;
        return inputVector;
    }
}
