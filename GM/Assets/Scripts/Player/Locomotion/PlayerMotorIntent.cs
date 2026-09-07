using UnityEngine;

namespace GhostMemory.Player
{
    public readonly struct PlayerMotorIntent
    {
        public Vector2 MoveInput { get; }
        public float TargetSpeed { get; }
        public bool WantCrouch { get; }
        public bool JumpRequested { get; }

        public PlayerMotorIntent(Vector2 moveInput, float targetSpeed, bool wantCrouch, bool jumpRequested)
        {
            MoveInput = moveInput;
            TargetSpeed = targetSpeed;
            WantCrouch = wantCrouch;
            JumpRequested = jumpRequested;
        }

        public static PlayerMotorIntent Still(bool wantCrouch)
        {
            return new PlayerMotorIntent(Vector2.zero, 0f, wantCrouch, false);
        }
    }
}
