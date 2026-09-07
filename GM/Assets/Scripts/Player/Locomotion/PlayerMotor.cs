using UnityEngine;

namespace GhostMemory.Player
{
    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(-50)]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] float walkSpeed = 5f;
        [SerializeField] float sprintSpeed = 8.5f;
        [SerializeField] float crouchSpeed = 2.4f;
        [SerializeField] float acceleration = 14f;
        [SerializeField] float airAcceleration = 3.5f;

        [Header("Jump")]
        [SerializeField] float jumpHeight = 1.2f;
        [SerializeField] float gravity = -22f;
        [SerializeField] float coyoteTime = 0.12f;
        [SerializeField] float jumpBufferTime = 0.1f;

        [Header("Crouch")]
        [SerializeField] bool crouchToggle;
        [SerializeField] float standingHeight = 1.8f;
        [SerializeField] float crouchHeight = 1.15f;
        [SerializeField] float heightLerpSpeed = 14f;
        [SerializeField] LayerMask ceilingMask = ~0;

        CharacterController _controller;
        Vector3 _horizontalVelocity;
        float _verticalVelocity;
        float _coyoteTimer;
        float _jumpBufferTimer;
        static readonly RaycastHit[] OverheadHits = new RaycastHit[8];

        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float CrouchSpeed => crouchSpeed;
        public bool CrouchToggle => crouchToggle;

        public bool IsGrounded { get; private set; }
        public bool IsCrouched { get; private set; }
        public bool JumpedThisFrame { get; private set; }
        public bool CanJump => IsGrounded || _coyoteTimer > 0f;
        public float NormalizedSpeed { get; private set; }
        public float VerticalVelocity => _verticalVelocity;
        public Vector3 HorizontalVelocity => _horizontalVelocity;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            ApplyHeight(standingHeight);
        }

        public void Tick(in PlayerMotorIntent intent, float deltaTime)
        {
            if (_controller == null || deltaTime <= 0f)
                return;

            JumpedThisFrame = false;
            IsGrounded = _controller.isGrounded;

            ApplyCrouch(intent.WantCrouch, deltaTime);
            ApplyHorizontal(intent, deltaTime);
            ApplyVertical(intent, deltaTime);

            Vector3 motion = _horizontalVelocity;
            motion.y = _verticalVelocity;
            CollisionFlags flags = _controller.Move(motion * deltaTime);

            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
                _verticalVelocity = 0f;

            float maxSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            NormalizedSpeed = maxSpeed > 0.01f
                ? Mathf.Clamp01(new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z).magnitude / maxSpeed)
                : 0f;
        }

        public void Stop()
        {
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = 0f;
            _jumpBufferTimer = 0f;
            NormalizedSpeed = 0f;
        }

        public bool CanStandUp()
        {
            if (_controller == null)
                return true;

            float radius = Mathf.Max(0.05f, _controller.radius * 0.9f);
            float currentTop = _controller.height;
            float extra = standingHeight - currentTop;
            if (extra <= 0.02f)
                return true;

            Vector3 origin = transform.position + Vector3.up * (currentTop - radius);
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                Vector3.up,
                OverheadHits,
                extra,
                ceilingMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = OverheadHits[i].collider;
                if (hitCollider == null)
                    continue;
                if (hitCollider.gameObject == gameObject)
                    continue;
                if (hitCollider.transform.IsChildOf(transform))
                    continue;
                return false;
            }

            return true;
        }

        void ApplyHorizontal(in PlayerMotorIntent intent, float deltaTime)
        {
            Vector2 move = Vector2.ClampMagnitude(intent.MoveInput, 1f);
            Vector3 wish = transform.right * move.x + transform.forward * move.y;
            float targetSpeed = Mathf.Max(0f, intent.TargetSpeed);
            float accel = IsGrounded ? acceleration : airAcceleration;
            float step = accel * Mathf.Max(targetSpeed, walkSpeed) * deltaTime;

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                wish * targetSpeed,
                step);
        }

        void ApplyVertical(in PlayerMotorIntent intent, float deltaTime)
        {
            if (intent.JumpRequested)
                _jumpBufferTimer = jumpBufferTime;
            else
                _jumpBufferTimer -= deltaTime;

            if (IsGrounded)
            {
                _coyoteTimer = coyoteTime;
                if (_verticalVelocity < 0f)
                    _verticalVelocity = -2f;
            }
            else
            {
                _coyoteTimer -= deltaTime;
                _verticalVelocity += gravity * deltaTime;
            }

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f && !IsCrouched)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                IsGrounded = false;
                JumpedThisFrame = true;
            }
        }

        void ApplyCrouch(bool wantCrouch, float deltaTime)
        {
            bool shouldCrouch = wantCrouch;
            if (!shouldCrouch && IsCrouched && !CanStandUp())
                shouldCrouch = true;

            float targetHeight = shouldCrouch ? crouchHeight : standingHeight;
            float height = Mathf.MoveTowards(_controller.height, targetHeight, heightLerpSpeed * deltaTime);
            ApplyHeight(height);

            IsCrouched = _controller.height < standingHeight - 0.02f;
        }

        void ApplyHeight(float height)
        {
            float clamped = Mathf.Max(height, _controller.radius * 2f + 0.05f);
            _controller.height = clamped;
            _controller.center = new Vector3(0f, clamped * 0.5f, 0f);
        }
    }
}
