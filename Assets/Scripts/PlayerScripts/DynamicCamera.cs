using UnityEngine;
using Cinemachine;

namespace GoodbyeBuddy {

    public class DynamicCamera : MonoBehaviour
    {
        public CinemachineVirtualCamera virtualCamera;
        private CinemachineFramingTransposer _framingTransposer;
        private PlayerController _playerController;
        private Rigidbody2D _playerRigidbody;

        public float offsetRight = 1f; // Desplazamiento horizontal hacia la derecha
        public float offsetLeft = -1f; // Desplazamiento horizontal hacia la izquierda
        public float verticalOffsetAdjustment = 0f; // Ajuste vertical manual para centrar en Y

        public float maxHorizontalDisplacement = 0.5f; // Máximo desplazamiento extra mientras camina
        public float smoothReturnSpeed = 2f; // Velocidad de retorno al punto inicial

        private float targetOffsetX;

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

            // Establecer el valor inicial del offset X
            targetOffsetX = _framingTransposer.m_TrackedObjectOffset.x;
        }

        private void Update()
        {
            // Ajuste horizontal según la dirección del jugador
            //float baseOffsetX = _playerController.IsFacingRight ? offsetRight : offsetLeft;

            if (Mathf.Abs(_playerRigidbody.velocity.x) > 0.1f)
            {
                // Permitir desplazamiento extra mientras camina
                //targetOffsetX = baseOffsetX + (_playerController.IsFacingRight ? maxHorizontalDisplacement : -maxHorizontalDisplacement);
            }
            else
            {
                // Suavemente regresar al offset base cuando se detiene
                //targetOffsetX = baseOffsetX;
            }

            // Aplicar suavemente el offset horizontal
            _framingTransposer.m_TrackedObjectOffset.x = Mathf.Lerp(
                _framingTransposer.m_TrackedObjectOffset.x,
                targetOffsetX,
                Time.deltaTime * smoothReturnSpeed
            );

            // Ajuste vertical dinámico
            _framingTransposer.m_TrackedObjectOffset.y = GetPlayerVerticalCenter() + verticalOffsetAdjustment;
        }

        private float GetPlayerVerticalCenter()
        {
            // Calcula el centro vertical del jugador (basado en su Collider2D o su Transform)
            Collider2D playerCollider = _playerController.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                return playerCollider.bounds.center.y - _playerController.transform.position.y;
            }

            // Si no hay Collider2D, asumimos el centro del Transform
            return 0f;
        }
    }
}
