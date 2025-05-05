using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : SingletonPersistent<GameInputManager>
{
    private PlayerInputActions playerInputActions;
    [HideInInspector] public bool mouseIsLastUsedDevice = true;
    Vector2 virtualCursorPos;

    public event EventHandler OnShootingAction, OnGamePausedAction, OnInteractAction, OnUsingAbilityAction, OnNotShootingAction, OnReloadAction;

    protected override void Awake()
    {
        base.Awake();
        
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        playerInputActions.player.shoot.started += OnPlayerShooting;
        playerInputActions.player.shoot.canceled += OnPlayerNotShooting;
        playerInputActions.player.pause.performed += OnGamePaused;
        playerInputActions.player.interact.performed += OnPlayerInteracting;
        playerInputActions.player.ability.performed += OnPlayerUsingAbility;
        playerInputActions.player.reload.performed += OnPlayerReloading;
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnPlayerUsingAbility(InputAction.CallbackContext context)
    {
        OnUsingAbilityAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerShooting(InputAction.CallbackContext context)
    {
        OnShootingAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerNotShooting(InputAction.CallbackContext context)
    {
        OnNotShootingAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnGamePaused(InputAction.CallbackContext context)
    {
        OnGamePausedAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerInteracting(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerReloading(InputAction.CallbackContext context)
    {
        OnReloadAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var _inputVector = playerInputActions.player.move.ReadValue<Vector2>();

        _inputVector = _inputVector.normalized;
        return _inputVector;
    }

    public Vector3 GetAimingVector()
    {
        Vector2 _mouseDelta = Mouse.current.delta.ReadValue();
        Vector2 _stickInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        
        if (_stickInput.sqrMagnitude > 0.01f)
        {
            mouseIsLastUsedDevice = false;
        }
        else if (_mouseDelta.sqrMagnitude > 0.01f)
        {
            mouseIsLastUsedDevice = true;
        }
        
        if (!mouseIsLastUsedDevice)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            Vector2 _readValue = Gamepad.current.rightStick.ReadValue();
            Vector3 _playerScreenPos = PlayerBehaviour.Instance.weaponBehaviour.mainCamera.WorldToScreenPoint(PlayerBehaviour.Instance.transform.position);
            Vector3 _aimScreenPos = _playerScreenPos + new Vector3(_readValue.x, _readValue.y, 0f) * Screen.width;

            return PlayerBehaviour.Instance.weaponBehaviour.mainCamera.ScreenToWorldPoint(new Vector3(_aimScreenPos.x, _aimScreenPos.y, _playerScreenPos.z));
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        return PlayerBehaviour.Instance.weaponBehaviour.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
