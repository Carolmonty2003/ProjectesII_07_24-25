using UnityEngine;

public class PlataformaToggle : MonoBehaviour
{
    [SerializeField] private string grupo = "A"; // Grupo de la plataforma: "A" o "B"

    private Renderer platformRenderer; // Para controlar la visibilidad
    private Collider2D platformCollider; // Para controlar el colisionador
    private static bool grupoAVisible = true; // Estado inicial: Grupo A visible, Grupo B invisible
    private bool isVisible; // Estado actual de la plataforma

    void Start()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider2D>();

        // Determinar visibilidad inicial basada en el grupo
        isVisible = grupo == "A" ? grupoAVisible : !grupoAVisible;

        UpdatePlatformState();
    }

    public static void AlternarGrupos()
    {
        // Cambiar el estado global del grupo A
        grupoAVisible = !grupoAVisible;

        // Actualizar todas las plataformas en la escena
        PlataformaToggle[] plataformas = FindObjectsOfType<PlataformaToggle>();
        foreach (var plataforma in plataformas)
        {
            plataforma.UpdateGroupState();
        }
    }

    private void UpdateGroupState()
    {
        // Cambiar visibilidad según el estado del grupo
        isVisible = grupo == "A" ? grupoAVisible : !grupoAVisible;
        UpdatePlatformState();
    }

    private void UpdatePlatformState()
    {
        // Actualizar visibilidad y colisión
        platformRenderer.enabled = isVisible;
        platformCollider.enabled = isVisible;
    }
}
