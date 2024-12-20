using UnityEngine;
using Cinemachine;
using System.Collections;


namespace GoodbyeBuddy
{
    public class DynamicCamera : MonoBehaviour
    {
        public CinemachineVirtualCamera virtualCamera;
        private CinemachineFramingTransposer _framingTransposer;
        private PlayerController _playerController;

        // Configuración de Zoom
        public float zoomNormal = 5f;
        public float zoomAumentado2 = 20f;
        public float zoomAumentado = 7f;
        public float zoomReducido1 = 4f;
        public float zoomReducido2 = 3f;
        public float zoomReducido3 = 2f;
        public float zoomLerpSpeed = 5f;
        public float zoomDelay = 2f;
        private float _zoomVelocity;

        private float _currentZoomTarget;
        private bool isZoomCoroutineRunning = false;

        // Configuración de offsets
        public float offsetRight = 0.5f;
        public float offsetLeft = -0.5f;
        public float verticalOffsetAdjustment = -0.5f;
        public float smoothReturnSpeed = 1.8f;

        private SpriteRenderer _playerSpriteRenderer; // Para acceder al flipX del jugador

        private void Start()
        {
            _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _playerController = FindObjectOfType<PlayerController>();
            _playerSpriteRenderer = _playerController.GetComponent<SpriteRenderer>();

            if (_framingTransposer == null)
                Debug.LogError("Framing Transposer no encontrado en la cámara virtual.");
            if (_playerController == null)
                Debug.LogError("No se encontró PlayerController.");
            if (_playerSpriteRenderer == null)
                Debug.LogError("No se encontró SpriteRenderer en el PlayerController.");

            if (virtualCamera.m_Lens.Orthographic)
            {
                _currentZoomTarget = virtualCamera.m_Lens.OrthographicSize; // Tamaño inicial de la cámara
            }
            else
            {
                Debug.LogError("Esta implementación funciona con cámaras ortográficas.");
            }
        }

        private void Update()
        {
            UpdateHorizontalOffset();
            UpdateVerticalOffset();
            UpdateCameraZoom();
        }

        private void UpdateHorizontalOffset()
        {
            // Determina la dirección del jugador usando flipX
            bool isFacingRight = !_playerSpriteRenderer.flipX;

            // Ajusta el desplazamiento horizontal basado en la dirección
            _framingTransposer.m_TrackedObjectOffset.x = isFacingRight ? offsetRight : offsetLeft;
        }

        private void UpdateVerticalOffset()
        {
            float playerVelocityY = _playerController.GetComponent<Rigidbody2D>().velocity.y;

            // Ajuste del desplazamiento vertical basado en la velocidad y centro del personaje
            float verticalTarget = verticalOffsetAdjustment;
            _framingTransposer.m_TrackedObjectOffset.y = Mathf.Lerp(
                _framingTransposer.m_TrackedObjectOffset.y,
                verticalTarget,
                Time.deltaTime * smoothReturnSpeed
            );
        }

        private void UpdateCameraZoom()
        {
            // Determina el zoom basado en el tamaño actual del personaje
            float newZoomTarget = zoomNormal;

            if (_playerController.EstaEnNivelDeReduccion(3))
            {
                newZoomTarget = zoomReducido3;
            }

            if (_playerController.EstaEnNivelDeReduccion(2))
            {
                newZoomTarget = zoomReducido2;
            }
            else if (_playerController.EstaEnNivelDeReduccion(1))
            {
                newZoomTarget = zoomReducido1;
            }
            else if (_playerController.EsGrande())
            {
                newZoomTarget = zoomAumentado;
            }
            else if (_playerController.EresMuyGrande())
            {
                newZoomTarget = zoomAumentado2;
            }

            if (Mathf.Abs(_currentZoomTarget - newZoomTarget) > 0.01f && !isZoomCoroutineRunning)
            {
                StartCoroutine(DelayedZoom(newZoomTarget));
            }

            virtualCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(
                virtualCamera.m_Lens.OrthographicSize,
                _currentZoomTarget,
                ref _zoomVelocity,
                zoomLerpSpeed
            );
        }


        private IEnumerator DelayedZoom(float newZoomTarget)
        {
            isZoomCoroutineRunning = true;

            yield return new WaitForSeconds(zoomDelay);


            _currentZoomTarget = newZoomTarget;

            isZoomCoroutineRunning = false;
        }
    }
}
