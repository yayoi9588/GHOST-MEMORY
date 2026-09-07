namespace GhostMemory.Player
{
    public class PlayerSprintingState : PlayerGroundedState
    {
        public PlayerSprintingState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Sprinting;

        protected override IPlayerState GetGroundedTransition(in PlayerInputState input, float deltaTime)
        {
            if (Machine.WantsCrouch)
                return Machine.Crouching;

            if (!Machine.HasMoveInput(input))
                return Machine.Idle;

            if (!Machine.CanSprint(input))
                return Machine.Walking;

            return null;
        }
    }
}
