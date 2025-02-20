using System;
using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    public event EventHandler OnShootingAction, OnGamePausedAction, OnInteractAction, OnUsingAbilityAction, OnSprintingAction, OnNotShootingAction;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        playerInputActions.player.sprint.started += OnPlayerSliding;
        playerInputActions.player.shoot.started += OnPlayerShooting;
        playerInputActions.player.shoot.canceled += OnPlayerNotShooting;
        playerInputActions.player.pause.performed += OnGamePaused;
        playerInputActions.player.interact.performed += OnPlayerInteracting;
        playerInputActions.player.ability.performed += OnPlayerUsingAbility;
    }
    /// <summary>
    /// Saves every player input as an event to fire off when performed
    /// </summary>
    /// <param name="context"></param>
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

    //Reads movement vector for every direction from the input
    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = playerInputActions.player.move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;
        return inputVector;
    }
}
