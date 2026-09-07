namespace GhostMemory.Player
{
    public class PlayerCrouchingState : PlayerGroundedState
    {
        public PlayerCrouchingState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Crouching;

        protected override IPlayerState GetGroundedTransition(in PlayerInputState input, float deltaTime)
        {
            if (Machine.WantsCrouch)
                return null;

            if (Motor != null && !Motor.CanStandUp())
                return null;

            return Machine.HasMoveInput(input) ? (IPlayerState)Machine.Walking : Machine.Idle;
        }
    }
}
