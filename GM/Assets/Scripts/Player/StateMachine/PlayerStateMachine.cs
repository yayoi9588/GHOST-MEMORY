using System;

namespace GhostMemory.Player
{
    public class PlayerStateMachine
    {
        const float MoveDeadzone = 0.1f;

        public PlayerStateMachine(PlayerMotor motor, PlayerStatus status)
        {
            Motor = motor;
            Status = status;

            Idle = new PlayerIdleState(this);
            Walking = new PlayerWalkingState(this);
            Sprinting = new PlayerSprintingState(this);
            Crouching = new PlayerCrouchingState(this);
            Airborne = new PlayerAirborneState(this);
            Dead = new PlayerDeadState(this);
        }

        public PlayerMotor Motor { get; }
        public PlayerStatus Status { get; }

        public PlayerIdleState Idle { get; }
        public PlayerWalkingState Walking { get; }
        public PlayerSprintingState Sprinting { get; }
        public PlayerCrouchingState Crouching { get; }
        public PlayerAirborneState Airborne { get; }
        public PlayerDeadState Dead { get; }

        public IPlayerState Current { get; private set; }
        public PlayerStateId CurrentId { get; private set; } = PlayerStateId.Idle;
        public PlayerStateId PreviousId { get; private set; } = PlayerStateId.Idle;
        public float TimeInState { get; private set; }
        public bool WantsCrouch { get; private set; }

        public bool IsSprinting => CurrentId == PlayerStateId.Sprinting;
        public bool IsDead => CurrentId == PlayerStateId.Dead;

        public event Action<PlayerStateId, PlayerStateId> StateChanged;

        public void Initialize()
        {
            Current = Idle;
            CurrentId = Idle.Id;
            PreviousId = Idle.Id;
            TimeInState = 0f;
            Current.Enter();
        }

        public void Tick(in PlayerInputState input, float deltaTime)
        {
            if (Current == null)
                Initialize();

            UpdateCrouchWish(input);

            IPlayerState next = Current.GetNextState(input, deltaTime);
            if (next != null && next != Current)
                ChangeState(next);

            TimeInState += deltaTime;
            Current.UpdateState(input, deltaTime);
        }

        public void ChangeState(IPlayerState next)
        {
            if (next == null || next == Current)
                return;

            Current?.Exit();

            PreviousId = CurrentId;
            Current = next;
            CurrentId = next.Id;
            TimeInState = 0f;
            Current.Enter();

            StateChanged?.Invoke(PreviousId, CurrentId);
        }

        public void SetCrouchWish(bool value)
        {
            WantsCrouch = value;
        }

        public IPlayerState ResolveGroundedState(in PlayerInputState input)
        {
            if (WantsCrouch)
                return Crouching;
            if (HasMoveInput(input))
                return CanSprint(input) ? (IPlayerState)Sprinting : Walking;
            return Idle;
        }

        public bool HasMoveInput(in PlayerInputState input)
        {
            return input.Move.sqrMagnitude > MoveDeadzone * MoveDeadzone;
        }

        public bool CanSprint(in PlayerInputState input)
        {
            return input.SprintHeld && !WantsCrouch && input.Move.y > MoveDeadzone;
        }

        public float GetGroundedSpeed(PlayerStateId stateId)
        {
            if (Motor == null)
                return 0f;

            switch (stateId)
            {
                case PlayerStateId.Crouching: return Motor.CrouchSpeed;
                case PlayerStateId.Sprinting: return Motor.SprintSpeed;
                default: return Motor.WalkSpeed;
            }
        }

        void UpdateCrouchWish(in PlayerInputState input)
        {
            if (Status != null && Status.IsDead)
            {
                WantsCrouch = false;
                return;
            }

            if (Motor != null && Motor.CrouchToggle)
            {
                if (input.CrouchPressed)
                    WantsCrouch = !WantsCrouch;
                return;
            }

            WantsCrouch = input.CrouchHeld;
        }
    }
}
