using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostMemory.Player
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;
        [SerializeField] string actionMapName = "Player";

        InputActionMap _map;
        InputAction _move;
        InputAction _look;
        InputAction _jump;
        InputAction _sprint;
        InputAction _crouch;
        InputAction _attack;
        InputAction _interact;

        public PlayerInputState Current { get; private set; }

        void Awake()
        {
            ResolveActions();
        }

        void OnEnable()
        {
            _map?.Enable();
        }

        void OnDisable()
        {
            _map?.Disable();
            Current = default;
        }

        void Update()
        {
            if (_map == null)
            {
                Current = default;
                return;
            }

            bool isPointerLook = _look != null
                && _look.activeControl != null
                && _look.activeControl.device is Pointer;

            Current = new PlayerInputState(
                _move != null ? _move.ReadValue<Vector2>() : Vector2.zero,
                _look != null ? _look.ReadValue<Vector2>() : Vector2.zero,
                _jump != null && _jump.WasPressedThisFrame(),
                _sprint != null && _sprint.IsPressed(),
                _crouch != null && _crouch.IsPressed(),
                _crouch != null && _crouch.WasPressedThisFrame(),
                _attack != null && _attack.WasPressedThisFrame(),
                _attack != null && _attack.IsPressed(),
                _interact != null && _interact.WasPressedThisFrame(),
                isPointerLook);
        }

        void ResolveActions()
        {
            if (actions == null)
                actions = InputSystem.actions;

            if (actions == null)
            {
                Debug.LogError("PlayerInputReader: InputActionAsset is not assigned.", this);
                return;
            }

            _map = actions.FindActionMap(actionMapName, throwIfNotFound: false);
            if (_map == null)
            {
                Debug.LogError($"PlayerInputReader: Action map '{actionMapName}' was not found.", this);
                return;
            }

            _move = _map.FindAction("Move");
            _look = _map.FindAction("Look");
            _jump = _map.FindAction("Jump");
            _sprint = _map.FindAction("Sprint");
            _crouch = _map.FindAction("Crouch");
            _attack = _map.FindAction("Attack");
            _interact = _map.FindAction("Interact");
        }

        void Reset()
        {
            actions = InputSystem.actions;
        }
    }
}
