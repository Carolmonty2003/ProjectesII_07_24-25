using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraFollowEstatua : MonoBehaviour
{
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera CameraEstatua;
    private CinemachineFramingTransposer _framingTransposer;
    private PlayerController _playerController;

    private SpriteRenderer _playerSpriteRenderer;

    private float _currentZoomTarget;
    public float offsetRight = 1.0f;
    public float offsetLeft = -1.0f;

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
            Debug.LogError("Framing Transposer no encontrado en la cámara virtual.");
        if (_playerController == null)
            Debug.LogError("No se encontró PlayerController.");
        if (_playerSpriteRenderer == null)
            Debug.LogError("No se encontró SpriteRenderer en el PlayerController.");

        if (CameraEstatua.m_Lens.Orthographic)
        {
            _currentZoomTarget = CameraEstatua.m_Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError("Esta implementación funciona con cámaras ortográficas.");
        }
    }

    private void Update()
    {
        UpdateHorizontalOffset();
    }

    private void UpdateHorizontalOffset()
    {
        // Determina la dirección del jugador usando flipX
        bool isFacingRight = !_playerSpriteRenderer.flipX;

        // Ajusta el desplazamiento horizontal basado en la dirección
        _framingTransposer.m_TrackedObjectOffset.x = isFacingRight ? offsetRight : offsetLeft;
    }
}
