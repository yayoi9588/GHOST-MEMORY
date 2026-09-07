using UnityEngine;

namespace GhostMemory.Player
{
    [DefaultExecutionOrder(100)]
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] Transform cameraPivot;

        [Header("Sensitivity")]
        [SerializeField] float mouseSensitivity = 0.12f;
        [SerializeField] float gamepadSensitivity = 140f;
        [SerializeField] bool invertY;

        [Header("Limits")]
        [SerializeField] float minPitch = -85f;
        [SerializeField] float maxPitch = 85f;

        [Header("Camera")]
        [SerializeField] float standingEyeHeight = 1.6f;
        [SerializeField] float crouchEyeHeight = 0.95f;
        [SerializeField] float eyeLerpSpeed = 14f;
        [SerializeField] float sprintFovKick = 8f;
        [SerializeField] float fovLerpSpeed = 10f;

        [Header("Head Bob")]
        [SerializeField] bool enableHeadBob = true;
        [SerializeField] float bobAmplitude = 0.035f;
        [SerializeField] float bobFrequency = 11f;

        Camera _camera;
        float _pitch;
        float _eyeHeight;
        float _bobTimer;
        float _baseFov;

        public Transform CameraPivot => cameraPivot;

        void Awake()
        {
            if (cameraPivot == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null)
                    cameraPivot = childCamera.transform;
            }

            if (cameraPivot != null)
            {
                _camera = cameraPivot.GetComponent<Camera>();
                cameraPivot.localPosition = new Vector3(0f, standingEyeHeight, 0f);
            }

            _eyeHeight = standingEyeHeight;
            _baseFov = _camera != null ? _camera.fieldOfView : 75f;
        }

        public void Tick(PlayerInputState input, bool isCrouching, bool isSprinting, float normalizedSpeed, bool isGrounded, float deltaTime)
        {
            if (cameraPivot == null)
                return;

            float yawDelta;
            float pitchDelta;
            if (input.IsPointerLook)
            {
                yawDelta = input.Look.x * mouseSensitivity;
                pitchDelta = input.Look.y * mouseSensitivity;
            }
            else
            {
                yawDelta = input.Look.x * gamepadSensitivity * deltaTime;
                pitchDelta = input.Look.y * gamepadSensitivity * deltaTime;
            }

            if (invertY)
                pitchDelta = -pitchDelta;

            transform.Rotate(0f, yawDelta, 0f);
            _pitch = Mathf.Clamp(_pitch - pitchDelta, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            float targetEye = isCrouching ? crouchEyeHeight : standingEyeHeight;
            _eyeHeight = Mathf.Lerp(_eyeHeight, targetEye, 1f - Mathf.Exp(-eyeLerpSpeed * deltaTime));

            Vector3 bob = Vector3.zero;
            if (enableHeadBob && isGrounded && normalizedSpeed > 0.1f)
            {
                _bobTimer += deltaTime * bobFrequency * (isSprinting ? 1.35f : 1f);
                bob.y = Mathf.Sin(_bobTimer) * bobAmplitude * normalizedSpeed;
                bob.x = Mathf.Cos(_bobTimer * 0.5f) * bobAmplitude * 0.5f * normalizedSpeed;
            }
            else
            {
                _bobTimer = 0f;
            }

            cameraPivot.localPosition = new Vector3(bob.x, _eyeHeight + bob.y, 0f);

            if (_camera != null)
            {
                float targetFov = _baseFov + (isSprinting ? sprintFovKick : 0f);
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFov, 1f - Mathf.Exp(-fovLerpSpeed * deltaTime));
            }
        }

        public void SetPitch(float pitch)
        {
            _pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        void Reset()
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
                cameraPivot = childCamera.transform;
        }
    }
}
