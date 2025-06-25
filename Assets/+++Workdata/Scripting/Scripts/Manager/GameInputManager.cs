using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameInputManager : SingletonPersistent<GameInputManager>
{
    private PlayerInputActions playerInputActions;
    [HideInInspector] public bool mouseIsLastUsedDevice = true;
    private Vector2 aimScreenPosition;
    private Vector2 mouseDelta;
    private Vector2 rightStickInput;

    public event EventHandler OnShootingAction, OnGamePausedAction, OnInteractAction, OnUsingAbilityAction, OnNotShootingAction, OnReloadAction, OnMeleeWeaponAction, 
        OnSkipDialogueWithController, OnSprinting, OnNotSprinting;

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
        playerInputActions.player.melee.performed += OnPlayerMeleeAttack;
        playerInputActions.player.skipDialogueWithController.performed += OnPlayerSkipDialogueWithController;
        playerInputActions.player.sprint.started += OnPlayerSprinting;
        playerInputActions.player.sprint.canceled += OnPlayerNotSprinting;
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        CheckForCurrentInput();
    }

    public void SetNewButtonAsSelected(GameObject current)
    {
        if (!mouseIsLastUsedDevice)
        {
            EventSystem.current.SetSelectedGameObject(current);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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

    private void OnPlayerSprinting(InputAction.CallbackContext context)
    {
        OnSprinting?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerNotSprinting(InputAction.CallbackContext context)
    {
        OnNotSprinting?.Invoke(this, EventArgs.Empty);
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
    
    private void OnPlayerMeleeAttack(InputAction.CallbackContext context)
    {
        OnMeleeWeaponAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPlayerSkipDialogueWithController(InputAction.CallbackContext context)
    {
        OnSkipDialogueWithController?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var _inputVector = playerInputActions.player.move.ReadValue<Vector2>();

        _inputVector = _inputVector.normalized;
        return _inputVector;
    }

    public bool MouseIsLastUsedDevice
    {
        get => mouseIsLastUsedDevice;
        set
        {
            if (mouseIsLastUsedDevice != value)
            {
                mouseIsLastUsedDevice = value;
                OnInputDeviceChanged(value);
            }
        }
    }

    public void OnInputDeviceChanged(bool isMouse)
    {
        if(InputGraphicsManager.Instance != null)
            InputGraphicsManager.Instance.SetInputGraphics(isMouse);
    }

    private void CheckForCurrentInput()
    {
        Vector2 _leftStickInput = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
        mouseDelta = Mouse.current.delta.ReadValue();
        rightStickInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;

        if (rightStickInput.sqrMagnitude > 0.01f || _leftStickInput.sqrMagnitude > 0.01f)
        {
            MouseIsLastUsedDevice = false;
        }
        else if (mouseDelta.sqrMagnitude > 0.01f)
        {
            MouseIsLastUsedDevice = true;
        }
    }

    public Vector3 GetAimingVector()
    {        
        if (!mouseIsLastUsedDevice)
        {
            Vector3 _playerScreenPos = PlayerBehaviour.Instance.weaponBehaviour.mainCamera.WorldToScreenPoint(PlayerBehaviour.Instance.transform.position);
            
            if(rightStickInput.sqrMagnitude > 0.25f)
            {
                aimScreenPosition = _playerScreenPos + new Vector3(rightStickInput.x, rightStickInput.y, 0f).normalized * ((float)Screen.width / 6);
                Mouse.current.WarpCursorPosition(aimScreenPosition);
            }

            return PlayerBehaviour.Instance.weaponBehaviour.mainCamera.ScreenToWorldPoint(new Vector3(aimScreenPosition.x, aimScreenPosition.y, _playerScreenPos.z));
        }

        Cursor.lockState = CursorLockMode.Confined;
        return PlayerBehaviour.Instance.weaponBehaviour.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
