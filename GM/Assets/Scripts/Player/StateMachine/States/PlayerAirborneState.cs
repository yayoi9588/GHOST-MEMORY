namespace GhostMemory.Player
{
    public class PlayerAirborneState : PlayerStateBase
    {
        PlayerStateId _speedSource = PlayerStateId.Walking;

        public PlayerAirborneState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Airborne;

        public override void Enter()
        {
            _speedSource = Machine.PreviousId == PlayerStateId.Sprinting
                ? PlayerStateId.Sprinting
                : PlayerStateId.Walking;
        }

        public override IPlayerState GetNextState(in PlayerInputState input, float deltaTime)
        {
            if (IsDead)
                return Machine.Dead;

            if (Motor == null)
                return null;

            if (Motor.IsGrounded && Machine.TimeInState > 0f)
                return Machine.ResolveGroundedState(input);

            return null;
        }

        protected override PlayerMotorIntent BuildIntent(in PlayerInputState input)
        {
            return new PlayerMotorIntent(
                input.Move,
                Machine.GetGroundedSpeed(_speedSource),
                Machine.WantsCrouch,
                input.JumpPressed);
        }
    }
}
