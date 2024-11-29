using UnityEngine;

public class Boton : MonoBehaviour
{
    public enum TipoBoton { Crecer, Restablecer, Reducir1, Reducir2 }
    public TipoBoton tipoBoton;
    private SpriteRenderer spriteRenderer;

    public Color colorInactivo = Color.white;
    public Color colorActivo = Color.green;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetColor();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController jugador = other.GetComponent<PlayerController>();

            if (jugador != null)
            {
                switch (tipoBoton)
                {
                    case TipoBoton.Crecer:
                        if (!jugador.EsGrande()) // Solo crece si no es grande
                        {
                            jugador.Crecer();
                            ActivarBoton();
                        }
                        break;

                    case TipoBoton.Reducir1:
                        if (!jugador.EstaEnNivelDeReduccion(1)) // Verifica si no está ya reducido a nivel 1
                        {
                            jugador.ReducirANivel1(); // Llama a ReducirANivel1()
                            ActivarBoton();
                        }
                        break;

                    case TipoBoton.Reducir2:
                        if (!jugador.EstaEnNivelDeReduccion(2)) // Verifica si no está ya reducido a nivel 2
                        {
                            jugador.ReducirANivel2(); // Llama a ReducirANivel2()
                            ActivarBoton();
                        }
                        break;

                    case TipoBoton.Restablecer:
                        if (!jugador.EsNormal()) // Solo restablece si no está en tamaño original
                        {
                            jugador.RestablecerEstado();
                            ReactivarBotones();
                            ActivarBoton();
                        }
                        break;
                }
            }
        }
    }

    private void ActivarBoton()
    {
        // Cambia el color del botón para indicar que fue usado
        spriteRenderer.color = colorActivo;
    }

    private void ResetColor()
    {
        // Restablece el color del botón a su estado inactivo
        spriteRenderer.color = colorInactivo;
    }

    // Función pública para reactivar este botón
    public void Reactivar()
    {
        ResetColor();
    }

    // Reactiva todos los botones en la escena
    private void ReactivarBotones()
    {
        Boton[] botones = FindObjectsOfType<Boton>();
        foreach (Boton boton in botones)
        {
            boton.ResetColor();
        }
    }
}
