using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostMemory.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerLook))]
    [RequireComponent(typeof(PlayerStatus))]
    [DefaultExecutionOrder(-100)]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerMotor motor;
        [SerializeField] PlayerLook look;
        [SerializeField] PlayerStatus status;
        [SerializeField] bool lockCursorOnStart = true;
        [SerializeField] bool showCrosshair = true;
        [SerializeField] Color crosshairColor = new Color(1f, 1f, 1f, 0.85f);
        [SerializeField] float crosshairSize = 4f;

        public static PlayerController Local { get; private set; }

        public PlayerInputReader InputReader => input;
        public PlayerMotor Motor => motor;
        public PlayerLook Look => look;
        public PlayerStatus Status => status;
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerStateId State => StateMachine != null ? StateMachine.CurrentId : PlayerStateId.Idle;
        public bool CursorLocked { get; private set; }

        public event Action AttackPressed;
        public event Action InteractPressed;
        public event Action<PlayerStateId, PlayerStateId> StateChanged;

        void Awake()
        {
            if (Local != null && Local != this)
            {
                Debug.LogWarning("A second PlayerController was spawned. Local player remains the first instance.", this);
            }
            else
            {
                Local = this;
            }

            if (input == null) input = GetComponent<PlayerInputReader>();
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (look == null) look = GetComponent<PlayerLook>();
            if (status == null) status = GetComponent<PlayerStatus>();

            StateMachine = new PlayerStateMachine(motor, status);
            StateMachine.Initialize();
        }

        void OnEnable()
        {
            if (status != null)
                status.Died += OnDied;

            if (StateMachine != null)
                StateMachine.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            if (status != null)
                status.Died -= OnDied;

            if (StateMachine != null)
                StateMachine.StateChanged -= OnStateChanged;

            if (Local == this)
                Local = null;

            SetCursorLocked(false);
        }

        void Start()
        {
            if (lockCursorOnStart)
                SetCursorLocked(true);
        }

        void Update()
        {
            HandleCursorToggle();

            PlayerInputState state = input != null ? input.Current : default;
            float dt = Time.deltaTime;
            bool isDead = status != null && status.IsDead;

            if (StateMachine != null)
                StateMachine.Tick(state, dt);

            if (look != null)
            {
                look.Tick(
                    isDead ? default : state,
                    motor != null && motor.IsCrouched,
                    StateMachine != null && StateMachine.IsSprinting,
                    motor != null ? motor.NormalizedSpeed : 0f,
                    motor == null || motor.IsGrounded,
                    dt);
            }

            if (isDead)
                return;

            if (state.AttackPressed)
                AttackPressed?.Invoke();

            if (state.InteractPressed)
                InteractPressed?.Invoke();
        }

        void OnGUI()
        {
            if (!showCrosshair || !CursorLocked)
                return;

            Color previous = GUI.color;
            GUI.color = crosshairColor;
            float size = crosshairSize;
            Rect rect = new Rect(
                (Screen.width - size) * 0.5f,
                (Screen.height - size) * 0.5f,
                size,
                size);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public void SetCursorLocked(bool locked)
        {
            CursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void HandleCursorToggle()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
                return;
            }

            if (!CursorLocked
                && status != null
                && !status.IsDead
                && Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }
        }

        void OnStateChanged(PlayerStateId previous, PlayerStateId next)
        {
            StateChanged?.Invoke(previous, next);
        }

        void OnDied()
        {
            SetCursorLocked(false);
        }

        void Reset()
        {
            input = GetComponent<PlayerInputReader>();
            motor = GetComponent<PlayerMotor>();
            look = GetComponent<PlayerLook>();
            status = GetComponent<PlayerStatus>();
        }
    }
}
