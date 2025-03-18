using System;
using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    public event EventHandler OnShootingAction, OnGamePausedAction, OnInteractAction, OnUsingAbilityAction, OnSprintingAction, OnNotShootingAction, OnReloadAction;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        playerInputActions.player.slide.started += OnPlayerSliding;
        playerInputActions.player.shoot.started += OnPlayerShooting;
        playerInputActions.player.shoot.canceled += OnPlayerNotShooting;
        playerInputActions.player.pause.performed += OnGamePaused;
        playerInputActions.player.interact.performed += OnPlayerInteracting;
        playerInputActions.player.ability.performed += OnPlayerUsingAbility;
        playerInputActions.player.reload.performed += OnPlayerReloading;
    }

    private void OnPlayerUsingAbility(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnUsingAbilityAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerShooting(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnShootingAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerNotShooting(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnNotShootingAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerSliding(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnSprintingAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnGamePaused(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnGamePausedAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerInteracting(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerReloading(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnReloadAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var _inputVector = playerInputActions.player.move.ReadValue<Vector2>();

        _inputVector = _inputVector.normalized;
        return _inputVector;
    }
}
