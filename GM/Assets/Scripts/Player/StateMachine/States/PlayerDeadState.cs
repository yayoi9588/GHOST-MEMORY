namespace GhostMemory.Player
{
    public class PlayerDeadState : PlayerStateBase
    {
        public PlayerDeadState(PlayerStateMachine machine) : base(machine)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Dead;

        public override void Enter()
        {
            Machine.SetCrouchWish(false);
            Motor?.Stop();
        }

        public override IPlayerState GetNextState(in PlayerInputState input, float deltaTime)
        {
            return IsDead ? null : Machine.Idle;
        }

        protected override PlayerMotorIntent BuildIntent(in PlayerInputState input)
        {
            return PlayerMotorIntent.Still(false);
        }
    }
}
