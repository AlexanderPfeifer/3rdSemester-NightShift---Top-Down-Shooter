//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/+++Workdata/Input/PlayerInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""player"",
            ""id"": ""eaa7bf19-3f9d-4e27-8989-7f78dc208745"",
            ""actions"": [
                {
                    ""name"": ""move"",
                    ""type"": ""Value"",
                    ""id"": ""20f41443-c445-472a-af74-395426ed2875"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""aiming"",
                    ""type"": ""Value"",
                    ""id"": ""bf4279fd-da55-42b1-b9bb-c614de0b418d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""shoot"",
                    ""type"": ""Button"",
                    ""id"": ""ab4f16cf-1f8f-47d1-b90e-78714f4a9005"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""pause"",
                    ""type"": ""Button"",
                    ""id"": ""88c9602f-6ee3-478c-bb85-98165776752a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""interact"",
                    ""type"": ""Button"",
                    ""id"": ""24a68473-3cd3-444c-adb8-7e26a585dd97"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ability"",
                    ""type"": ""Button"",
                    ""id"": ""edbd92e8-301c-4bd2-8b99-411b62d050bb"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""reload"",
                    ""type"": ""Button"",
                    ""id"": ""7851258e-d290-4f4a-9014-6a400e7dc028"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""melee"",
                    ""type"": ""Button"",
                    ""id"": ""dc0d9b29-3f25-4876-8fa7-1fdcc01575bb"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""skipDialogueWithController"",
                    ""type"": ""Button"",
                    ""id"": ""a344e04d-e97a-4e63-a980-42a923ed31c7"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""sprint"",
                    ""type"": ""Button"",
                    ""id"": ""9568032a-1855-4751-93c8-fd77c74540b1"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""dbb4e3d8-0074-4827-93a9-2d060bbdfe7f"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""6d145ca1-4afd-45f3-9870-49e4dd63f4c9"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""65b48c92-7bb1-4cc0-bb51-df06b0246768"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""6b930f0c-76bc-4a41-89a6-9dd03f0fe9fa"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""f2f20665-a534-471b-b428-9e79f474be42"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""d2b566b8-d974-438d-8612-a5db513cb165"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6a7808bd-e5fd-43ca-9f55-442ed6f791c4"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""aiming"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1398ef25-0914-4bd9-ba03-945888294d2f"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""aiming"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d4fea68-2ccc-452b-8ee3-a436e46f9afa"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7382d56a-d8aa-49a2-9e23-578e2f5eb6ff"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""237162ee-cb7e-4d8b-8d9c-b09ac669746a"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7e419cc7-fdf4-484c-9747-4d00220f13d1"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""083e04d3-8ec6-4f99-8924-cdc3cef4c860"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ef782501-179d-4468-93bc-8e5af7bf370e"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ability"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1e185161-5c85-42dc-8c97-ca05be97645c"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""db82cc94-4186-4491-95fa-3909ebe5f651"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""reload"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a72f0a6b-56ca-4b98-8702-674dcd1607d2"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""reload"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b4372265-449b-4dca-bca5-4426e9cfc6ef"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""melee"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""720d94e3-9e76-4c8e-8ccb-01bfd4488ca6"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""melee"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""334b069c-096b-4237-bd93-d728e97c81d4"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""skipDialogueWithController"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7a2ebb04-9e05-464a-b143-95a572a237eb"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d97062f-dd41-4c37-954d-60b66833cef9"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // player
        m_player = asset.FindActionMap("player", throwIfNotFound: true);
        m_player_move = m_player.FindAction("move", throwIfNotFound: true);
        m_player_aiming = m_player.FindAction("aiming", throwIfNotFound: true);
        m_player_shoot = m_player.FindAction("shoot", throwIfNotFound: true);
        m_player_pause = m_player.FindAction("pause", throwIfNotFound: true);
        m_player_interact = m_player.FindAction("interact", throwIfNotFound: true);
        m_player_ability = m_player.FindAction("ability", throwIfNotFound: true);
        m_player_reload = m_player.FindAction("reload", throwIfNotFound: true);
        m_player_melee = m_player.FindAction("melee", throwIfNotFound: true);
        m_player_skipDialogueWithController = m_player.FindAction("skipDialogueWithController", throwIfNotFound: true);
        m_player_sprint = m_player.FindAction("sprint", throwIfNotFound: true);
    }

    ~@PlayerInputActions()
    {
        UnityEngine.Debug.Assert(!m_player.enabled, "This will cause a leak and performance issues, PlayerInputActions.player.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // player
    private readonly InputActionMap m_player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_player_move;
    private readonly InputAction m_player_aiming;
    private readonly InputAction m_player_shoot;
    private readonly InputAction m_player_pause;
    private readonly InputAction m_player_interact;
    private readonly InputAction m_player_ability;
    private readonly InputAction m_player_reload;
    private readonly InputAction m_player_melee;
    private readonly InputAction m_player_skipDialogueWithController;
    private readonly InputAction m_player_sprint;
    public struct PlayerActions
    {
        private @PlayerInputActions m_Wrapper;
        public PlayerActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @move => m_Wrapper.m_player_move;
        public InputAction @aiming => m_Wrapper.m_player_aiming;
        public InputAction @shoot => m_Wrapper.m_player_shoot;
        public InputAction @pause => m_Wrapper.m_player_pause;
        public InputAction @interact => m_Wrapper.m_player_interact;
        public InputAction @ability => m_Wrapper.m_player_ability;
        public InputAction @reload => m_Wrapper.m_player_reload;
        public InputAction @melee => m_Wrapper.m_player_melee;
        public InputAction @skipDialogueWithController => m_Wrapper.m_player_skipDialogueWithController;
        public InputAction @sprint => m_Wrapper.m_player_sprint;
        public InputActionMap Get() { return m_Wrapper.m_player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @move.started += instance.OnMove;
            @move.performed += instance.OnMove;
            @move.canceled += instance.OnMove;
            @aiming.started += instance.OnAiming;
            @aiming.performed += instance.OnAiming;
            @aiming.canceled += instance.OnAiming;
            @shoot.started += instance.OnShoot;
            @shoot.performed += instance.OnShoot;
            @shoot.canceled += instance.OnShoot;
            @pause.started += instance.OnPause;
            @pause.performed += instance.OnPause;
            @pause.canceled += instance.OnPause;
            @interact.started += instance.OnInteract;
            @interact.performed += instance.OnInteract;
            @interact.canceled += instance.OnInteract;
            @ability.started += instance.OnAbility;
            @ability.performed += instance.OnAbility;
            @ability.canceled += instance.OnAbility;
            @reload.started += instance.OnReload;
            @reload.performed += instance.OnReload;
            @reload.canceled += instance.OnReload;
            @melee.started += instance.OnMelee;
            @melee.performed += instance.OnMelee;
            @melee.canceled += instance.OnMelee;
            @skipDialogueWithController.started += instance.OnSkipDialogueWithController;
            @skipDialogueWithController.performed += instance.OnSkipDialogueWithController;
            @skipDialogueWithController.canceled += instance.OnSkipDialogueWithController;
            @sprint.started += instance.OnSprint;
            @sprint.performed += instance.OnSprint;
            @sprint.canceled += instance.OnSprint;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @move.started -= instance.OnMove;
            @move.performed -= instance.OnMove;
            @move.canceled -= instance.OnMove;
            @aiming.started -= instance.OnAiming;
            @aiming.performed -= instance.OnAiming;
            @aiming.canceled -= instance.OnAiming;
            @shoot.started -= instance.OnShoot;
            @shoot.performed -= instance.OnShoot;
            @shoot.canceled -= instance.OnShoot;
            @pause.started -= instance.OnPause;
            @pause.performed -= instance.OnPause;
            @pause.canceled -= instance.OnPause;
            @interact.started -= instance.OnInteract;
            @interact.performed -= instance.OnInteract;
            @interact.canceled -= instance.OnInteract;
            @ability.started -= instance.OnAbility;
            @ability.performed -= instance.OnAbility;
            @ability.canceled -= instance.OnAbility;
            @reload.started -= instance.OnReload;
            @reload.performed -= instance.OnReload;
            @reload.canceled -= instance.OnReload;
            @melee.started -= instance.OnMelee;
            @melee.performed -= instance.OnMelee;
            @melee.canceled -= instance.OnMelee;
            @skipDialogueWithController.started -= instance.OnSkipDialogueWithController;
            @skipDialogueWithController.performed -= instance.OnSkipDialogueWithController;
            @skipDialogueWithController.canceled -= instance.OnSkipDialogueWithController;
            @sprint.started -= instance.OnSprint;
            @sprint.performed -= instance.OnSprint;
            @sprint.canceled -= instance.OnSprint;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @player => new PlayerActions(this);
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnAiming(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnAbility(InputAction.CallbackContext context);
        void OnReload(InputAction.CallbackContext context);
        void OnMelee(InputAction.CallbackContext context);
        void OnSkipDialogueWithController(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
    }
}
