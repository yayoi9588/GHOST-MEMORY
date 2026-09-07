namespace GhostMemory.Player
{
    public interface IPlayerState
    {
        PlayerStateId Id { get; }

        void Enter();

        void Exit();

        IPlayerState GetNextState(in PlayerInputState input, float deltaTime);

        void UpdateState(in PlayerInputState input, float deltaTime);
    }
}
