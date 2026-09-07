namespace GhostMemory.Player
{
    public abstract class PlayerGroundedState : PlayerStateBase
    {
        protected PlayerGroundedState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override IPlayerState GetNextState(in PlayerInputState input, float deltaTime)
        {
            if (IsDead)
                return Machine.Dead;

            if (Motor == null)
                return null;

            if (input.JumpPressed && Motor.CanJump && !Machine.WantsCrouch)
                return Machine.Airborne;

            if (!Motor.IsGrounded)
                return Machine.Airborne;

            return GetGroundedTransition(input, deltaTime);
        }

        protected abstract IPlayerState GetGroundedTransition(in PlayerInputState input, float deltaTime);
    }
}
