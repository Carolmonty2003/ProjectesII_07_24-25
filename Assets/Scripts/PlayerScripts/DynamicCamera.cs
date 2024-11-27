using UnityEngine;
using Cinemachine;

namespace GoodbyeBuddy
{
    public class DynamicCamera : MonoBehaviour
    {
        public CinemachineVirtualCamera virtualCamera;
        private CinemachineFramingTransposer _framingTransposer;
        private PlayerController _playerController;
        private Rigidbody2D _playerRigidbody;

        // Configuración para una cámara fluida y cinematográfica
        public float offsetRight = 1.0f;
        public float offsetLeft = -1.0f;
        public float verticalOffsetAdjustment = 0.2f;
        public float maxHorizontalDisplacement = 0.3f;
        public float maxVerticalOffset = 0.15f;
        public float smoothReturnSpeed = 1.8f;
        public float maxVerticalSpeed = 6.0f;

        private float targetOffsetX;
        private float targetOffsetY;

        private void Start()
        {
            _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _playerController = FindObjectOfType<PlayerController>();
            _playerRigidbody = _playerController.GetComponent<Rigidbody2D>();

            if (_framingTransposer == null)
                Debug.LogError("Framing Transposer no encontrado en la cámara virtual.");
            if (_playerController == null)
                Debug.LogError("No se encontró PlayerController.");
            if (_playerRigidbody == null)
                Debug.LogError("No se encontró Rigidbody2D en el PlayerController.");

            targetOffsetX = _framingTransposer.m_TrackedObjectOffset.x;
            targetOffsetY = _framingTransposer.m_TrackedObjectOffset.y;
        }

        private void Update()
        {
            UpdateHorizontalOffset();
            UpdateVerticalOffset();
            ApplyWeightedOffsets();
        }

        private void UpdateHorizontalOffset()
        {
            float baseOffsetX = _playerController.IsFacingRight ? offsetRight : offsetLeft;

            if (Mathf.Abs(_playerRigidbody.velocity.x) > 0.1f)
            {
                float displacementFactor = Mathf.Clamp(
                    _playerRigidbody.velocity.x / 15f, // Reducida influencia de la velocidad para suavidad
                    -maxHorizontalDisplacement,
                    maxHorizontalDisplacement
                );

                targetOffsetX = baseOffsetX + (_playerController.IsFacingRight ? displacementFactor : -displacementFactor);
            }
            else
            {
                targetOffsetX = baseOffsetX;
            }
        }

        private void UpdateVerticalOffset()
        {
            float playerVelocityY = _playerRigidbody.velocity.y;

            float verticalAdjustment = Mathf.Lerp(
                _framingTransposer.m_TrackedObjectOffset.y,
                GetPlayerVerticalCenter() + verticalOffsetAdjustment + (playerVelocityY * 0.05f), // Menor impacto de la velocidad
                Time.deltaTime * smoothReturnSpeed
            );

            targetOffsetY = Mathf.Clamp(verticalAdjustment, -maxVerticalOffset, maxVerticalOffset);
        }

        private void ApplyWeightedOffsets()
        {
            float horizontalWeight = Mathf.Clamp01(1 - Mathf.Abs(_playerRigidbody.velocity.y) / maxVerticalSpeed);
            float verticalWeight = 1f - horizontalWeight;

            _framingTransposer.m_TrackedObjectOffset.x = Mathf.Lerp(
                _framingTransposer.m_TrackedObjectOffset.x,
                targetOffsetX,
                Time.deltaTime * smoothReturnSpeed * horizontalWeight
            );

            _framingTransposer.m_TrackedObjectOffset.y = Mathf.Lerp(
                _framingTransposer.m_TrackedObjectOffset.y,
                targetOffsetY,
                Time.deltaTime * smoothReturnSpeed * verticalWeight
            );
        }

        private float GetPlayerVerticalCenter()
        {
            Collider2D playerCollider = _playerController.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                return playerCollider.bounds.center.y - _playerController.transform.position.y;
            }
            return 0f;
        }
    }
}
