namespace GhostMemory.Player
{
    public class PlayerIdleState : PlayerGroundedState
    {
        public PlayerIdleState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Idle;

        protected override IPlayerState GetGroundedTransition(in PlayerInputState input, float deltaTime)
        {
            if (Machine.WantsCrouch)
                return Machine.Crouching;

            if (Machine.HasMoveInput(input))
                return Machine.CanSprint(input) ? (IPlayerState)Machine.Sprinting : Machine.Walking;

            return null;
        }
    }
}
