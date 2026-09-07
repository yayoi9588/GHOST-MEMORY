namespace GhostMemory.Player
{
    public class PlayerWalkingState : PlayerGroundedState
    {
        public PlayerWalkingState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Walking;

        protected override IPlayerState GetGroundedTransition(in PlayerInputState input, float deltaTime)
        {
            if (Machine.WantsCrouch)
                return Machine.Crouching;

            if (!Machine.HasMoveInput(input))
                return Machine.Idle;

            if (Machine.CanSprint(input))
                return Machine.Sprinting;

            return null;
        }
    }
}
