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

        public float zoomNormal = 5f;  
        public float zoomAumentado = 7f;
        public float zoomReducido1 = 4f;
        public float zoomReducido2 = 3f;
        public float zoomLerpSpeed = 5f;

        private float _currentZoomTarget;

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

            _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _playerController = FindObjectOfType<PlayerController>();

            if (virtualCamera.m_Lens.Orthographic)
            {
                _currentZoomTarget = virtualCamera.m_Lens.OrthographicSize; // Tamaño inicial de la cámara
            }
            else
            {
                Debug.LogError("Esta implementación funciona con cámaras ortográficas.");
            }

            if (_playerController == null)
            {
                Debug.LogError("No se encontró el PlayerController.");
            }
        }

        private void Update()
        {
            UpdateHorizontalOffset();
            UpdateVerticalOffset();
            ApplyWeightedOffsets();
            UpdateCameraZoom();
        }

        private void UpdateHorizontalOffset()
        {
            // Determina si el personaje está mirando a la derecha o a la izquierda
            bool isFacingRight = _playerController.transform.localScale.x > 0;

            float baseOffsetX = isFacingRight ? offsetRight : offsetLeft;

            if (Mathf.Abs(_playerRigidbody.velocity.x) > 0.1f)
            {
                float displacementFactor = Mathf.Clamp(
                    _playerRigidbody.velocity.x / 15f, 
                    -maxHorizontalDisplacement,
                    maxHorizontalDisplacement
                );

                targetOffsetX = baseOffsetX + (isFacingRight ? displacementFactor : -displacementFactor);
            }
            else
            {
                targetOffsetX = baseOffsetX;
            }
        }

        private void UpdateVerticalOffset()
        {
            float playerVelocityY = _playerRigidbody.velocity.y;

            // Ajustar desplazamiento vertical basado en la velocidad y centro del personaje
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

            // Desplazamientos suavizados a la cámara
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

        private void UpdateCameraZoom()
        {
            // Determina el zoom basado en el tamaño actual del personaje
            if (_playerController.EstaEnNivelDeReduccion(2))
            {
                _currentZoomTarget = zoomReducido2; // Zoom máximo cuando está más pequeño
            }
            else if (_playerController.EstaEnNivelDeReduccion(1))
            {
                _currentZoomTarget = zoomReducido1; // Zoom intermedio
            }
            else if (_playerController.EsGrande())
            {
                _currentZoomTarget = zoomAumentado; // Zoom al crecer
            }
            else
            {
                _currentZoomTarget = zoomNormal; // Zoom normal
            }

            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                virtualCamera.m_Lens.OrthographicSize,
                _currentZoomTarget,
                Time.deltaTime * zoomLerpSpeed
            );
        }
    }
}