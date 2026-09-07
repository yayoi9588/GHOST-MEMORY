using UnityEngine;

namespace GhostMemory.Player
{
    public readonly struct PlayerInputState
    {
        public Vector2 Move { get; }
        public Vector2 Look { get; }
        public bool JumpPressed { get; }
        public bool SprintHeld { get; }
        public bool CrouchHeld { get; }
        public bool CrouchPressed { get; }
        public bool AttackPressed { get; }
        public bool AttackHeld { get; }
        public bool InteractPressed { get; }
        public bool IsPointerLook { get; }

        public PlayerInputState(
            Vector2 move,
            Vector2 look,
            bool jumpPressed,
            bool sprintHeld,
            bool crouchHeld,
            bool crouchPressed,
            bool attackPressed,
            bool attackHeld,
            bool interactPressed,
            bool isPointerLook)
        {
            Move = move;
            Look = look;
            JumpPressed = jumpPressed;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
            CrouchPressed = crouchPressed;
            AttackPressed = attackPressed;
            AttackHeld = attackHeld;
            InteractPressed = interactPressed;
            IsPointerLook = isPointerLook;
        }
    }
}
