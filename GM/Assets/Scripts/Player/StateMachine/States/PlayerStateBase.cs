namespace GhostMemory.Player
{
    public abstract class PlayerStateBase : IPlayerState
    {
        protected PlayerStateBase(PlayerStateMachine machine)
        {
            Machine = machine;
        }

        protected PlayerStateMachine Machine { get; }
        protected PlayerMotor Motor => Machine.Motor;
        protected PlayerStatus Status => Machine.Status;
        protected bool IsDead => Status != null && Status.IsDead;

        public abstract PlayerStateId Id { get; }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public abstract IPlayerState GetNextState(in PlayerInputState input, float deltaTime);

        public virtual void UpdateState(in PlayerInputState input, float deltaTime)
        {
            if (Motor == null)
                return;

            Motor.Tick(BuildIntent(input), deltaTime);
        }

        protected virtual PlayerMotorIntent BuildIntent(in PlayerInputState input)
        {
            return new PlayerMotorIntent(
                input.Move,
                Machine.GetGroundedSpeed(Id),
                Machine.WantsCrouch,
                input.JumpPressed);
        }
    }
}
