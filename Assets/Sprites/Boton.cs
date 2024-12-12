using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Boton : MonoBehaviour
{
    public enum TipoBoton { Crecer, Restablecer, Reducir1, Reducir2, Reducir3 }
    [SerializeField] private TipoBoton tipoBoton;

    [Header("Colores")]
    [SerializeField] private Color colorInactivo = Color.white;
    [SerializeField] private Color colorActivo = Color.green;
    [SerializeField] private float duracionTransicionColor = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Coroutine transicionCoroutine;
    private bool botonActivado;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetColor();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController jugador = other.GetComponent<PlayerController>();
        if (jugador == null) return;

        // Ejecuta la acci�n dependiendo del tipo de bot�n
        switch (tipoBoton)
        {
            case TipoBoton.Crecer:
                if (!jugador.EsGrande()) jugador.Crecer();
                break;
            case TipoBoton.Reducir1:
                if (!jugador.EstaEnNivelDeReduccion(1)) jugador.ReducirANivel1();
                break;
            case TipoBoton.Reducir2:
                if (!jugador.EstaEnNivelDeReduccion(2)) jugador.ReducirANivel2();
                break;
            case TipoBoton.Reducir3:
                if (!jugador.EstaEnNivelDeReduccion(3)) jugador.ReducirANivel3();
                break;
            case TipoBoton.Restablecer:
                if (!jugador.EsNormal()) jugador.RestablecerEstado();
                break;
        }

        ActivarBoton();
    }

    private void ActivarBoton()
    {
        if (botonActivado) return; // Evita activar el bot�n varias veces seguidas

        botonActivado = true;
        if (transicionCoroutine != null) StopCoroutine(transicionCoroutine);
        transicionCoroutine = StartCoroutine(TransicionColor(colorActivo));

        // Reinicia el color despu�s de un tiempo (por ejemplo, 2 segundos)
        Invoke(nameof(ResetColor), 2f);
    }

    private void ResetColor()
    {
        botonActivado = false; // Permite reactivar el bot�n
        if (transicionCoroutine != null) StopCoroutine(transicionCoroutine);
        transicionCoroutine = StartCoroutine(TransicionColor(colorInactivo));
    }

    private System.Collections.IEnumerator TransicionColor(Color targetColor)
    {
        Color inicio = spriteRenderer.color;
        float tiempo = 0f;

        // Lerp para suavizar la transici�n de color
        while (tiempo < duracionTransicionColor)
        {
            spriteRenderer.color = Color.Lerp(inicio, targetColor, tiempo / duracionTransicionColor);
            tiempo += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = targetColor;
    }
}
