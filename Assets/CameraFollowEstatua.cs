using Cinemachine;
using System.Collections;
using UnityEngine;

namespace GoodbyeBuddy
{
    public class CameraFollowEstatua : MonoBehaviour
    {
        public CinemachineVirtualCamera mainCamera;
        public CinemachineVirtualCamera CameraEstatua;
        private CinemachineFramingTransposer _framingTransposer;
        private PlayerController _playerController;

        private SpriteRenderer _playerSpriteRenderer;

        private float _currentZoomTarget;
        private bool isZoomCoroutineRunning = false;
        private float _zoomVelocity;

        public float offsetRight = 1.0f;
        public float offsetLeft = -1.0f;
        public float zoomNormal = 5f;
        public float zoomReducido3 = 2f;
        public float zoomLerpSpeed = 5f;
        public float zoomDelay = 2f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CameraEstatua.Priority = 10;
                mainCamera.Priority = 0;

                CameraEstatua.transform.position = new Vector3(
                    CameraEstatua.transform.position.x,
                    CameraEstatua.transform.position.y,
                    mainCamera.transform.position.z
                );
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                mainCamera.Priority = 10;
                CameraEstatua.Priority = 0;
            }
        }

        private void Start()
        {
            _framingTransposer = CameraEstatua.GetCinemachineComponent<CinemachineFramingTransposer>();
            _playerController = FindObjectOfType<PlayerController>();
            _playerSpriteRenderer = _playerController.GetComponent<SpriteRenderer>();

            if (_framingTransposer == null)
                Debug.LogError("Framing Transposer no encontrado en la c�mara virtual.");
            if (_playerController == null)
                Debug.LogError("No se encontr� PlayerController.");
            if (_playerSpriteRenderer == null)
                Debug.LogError("No se encontr� SpriteRenderer en el PlayerController.");

            if (CameraEstatua.m_Lens.Orthographic)
            {
                _currentZoomTarget = CameraEstatua.m_Lens.OrthographicSize;
            }
            else
            {
                Debug.LogError("Esta implementaci�n funciona con c�maras ortogr�ficas.");
            }
        }

        private void Update()
        {
            UpdateHorizontalOffset();
            UpdateCameraZoom();
        }

        private void UpdateHorizontalOffset()
        {
            // Determina la direcci�n del jugador usando flipX
            bool isFacingRight = !_playerSpriteRenderer.flipX;

            // Ajusta el desplazamiento horizontal basado en la direcci�n
            _framingTransposer.m_TrackedObjectOffset.x = isFacingRight ? offsetRight : offsetLeft;
        }

        private void UpdateCameraZoom()
        {
            // Determina el zoom basado en el tamaño actual del personaje
            float newZoomTarget = zoomNormal;

            if (_playerController.EstaEnNivelDeReduccion(3))
            {
                newZoomTarget = zoomReducido3;
            }

            if (Mathf.Abs(_currentZoomTarget - newZoomTarget) > 0.01f && !isZoomCoroutineRunning)
            {
                StartCoroutine(DelayedZoom(newZoomTarget));
            }

            CameraEstatua.m_Lens.OrthographicSize = Mathf.SmoothDamp(
                CameraEstatua.m_Lens.OrthographicSize,
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
