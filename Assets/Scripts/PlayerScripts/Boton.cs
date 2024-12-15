using UnityEngine; 

[RequireComponent(typeof(SpriteRenderer))]
public class Boton : MonoBehaviour
{
    public enum TipoBoton { Crecer, Crecer2, Restablecer, RestablecerDesdeCrecido2, Reducir1, Reducir2, Reducir3 }
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

        // Evitar interacción si el jugador está en Crecido2, excepto con RestablecerDesdeCrecido2
        if (jugador.EstaEnNivelDeReduccion(5) && tipoBoton != TipoBoton.RestablecerDesdeCrecido2) return;

        switch (tipoBoton)
        {
            case TipoBoton.Crecer:
                jugador.Crecer();
                break;
            case TipoBoton.Crecer2:
                jugador.CrecerANivel2();
                break;
            case TipoBoton.Reducir1:
                jugador.ReducirANivel1();
                break;
            case TipoBoton.Reducir2:
                jugador.ReducirANivel2();
                break;
            case TipoBoton.Reducir3:
                jugador.ReducirANivel3();
                break;
            case TipoBoton.Restablecer:
                jugador.RestablecerEstado();
                break;
            case TipoBoton.RestablecerDesdeCrecido2:
                jugador.RestablecerDesdeCrecido2();
                break;
        }

        ActivarBoton();
    }

    private void ActivarBoton()
    {
        if (botonActivado) return;

        botonActivado = true;
        if (transicionCoroutine != null) StopCoroutine(transicionCoroutine);
        transicionCoroutine = StartCoroutine(TransicionColor(colorActivo));

        Invoke(nameof(ResetColor), 2f);
    }

    private void ResetColor()
    {
        botonActivado = false;
        if (transicionCoroutine != null) StopCoroutine(transicionCoroutine);
        transicionCoroutine = StartCoroutine(TransicionColor(colorInactivo));
    }

    private System.Collections.IEnumerator TransicionColor(Color targetColor)
    {
        Color inicio = spriteRenderer.color;
        float tiempo = 0f;

        while (tiempo < duracionTransicionColor)
        {
            spriteRenderer.color = Color.Lerp(inicio, targetColor, tiempo / duracionTransicionColor);
            tiempo += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = targetColor;
    }
}
