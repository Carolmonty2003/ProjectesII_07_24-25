using UnityEngine;
using Cinemachine;

public class CameraLimits : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public Collider2D boundingShape;

    private void Start()
    {
        var confiner = virtualCamera.GetComponent<CinemachineConfiner>();
        if (confiner == null)
        {
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
        }

        if (!(boundingShape is PolygonCollider2D || boundingShape is CompositeCollider2D))
        {
            Debug.LogError("El boundingShape debe ser un PolygonCollider2D o CompositeCollider2D.");
            return;
        }

        confiner.m_BoundingShape2D = boundingShape;
        confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;

        confiner.InvalidatePathCache();
    }
}