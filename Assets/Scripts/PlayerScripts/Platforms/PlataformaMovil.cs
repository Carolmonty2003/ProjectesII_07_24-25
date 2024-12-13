using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{
    [Header("Puntos destino")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Configuración de movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private LayerMask capaJugador;
    [SerializeField] private LayerMask capaSuelo; // Asegúrate de incluir la capa suelo que contiene a la plataforma

    private Rigidbody2D rb;
    private PlayerController jugadorActual;
    private bool jugadorCerca;

    // Dirección del empuje: 1 hacia B, -1 hacia A, 0 sin empuje
    private int direccionEmpuje = 0;

    private Collider2D miCollider;
    private Transform puntoSueloJugador; // Referencia al puntoSuelo del jugador

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        miCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (jugadorCerca && jugadorActual != null && jugadorActual.EsGrande())
        {
            bool jugadorEncima = false;
            if (puntoSueloJugador != null)
            {
                // Revisamos si el puntoSuelo del jugador detecta esta plataforma
                Collider2D sueloDetectado = Physics2D.OverlapCircle(puntoSueloJugador.position, 0.2f, capaSuelo);
                if (sueloDetectado == miCollider)
                {
                    // El jugador está encima de la plataforma
                    jugadorEncima = true;
                }
            }

            if (jugadorEncima)
            {
                // Si el jugador está encima, no se mueve la plataforma
                direccionEmpuje = 0;
            }
            else
            {
                // Jugador está al costado, ver si empuja
                float inputHorizontal = Input.GetAxisRaw("Horizontal");
                if (inputHorizontal > 0.1f)
                {
                    direccionEmpuje = 1;
                }
                else if (inputHorizontal < -0.1f)
                {
                    direccionEmpuje = -1;
                }
                else
                {
                    direccionEmpuje = 0;
                }
            }
        }
        else
        {
            direccionEmpuje = 0;
        }

        MoverPlataforma();
    }

    private void MoverPlataforma()
    {
        if (direccionEmpuje == 1)
        {
            // Mover hacia B
            Vector3 nuevaPosicion = Vector3.MoveTowards(transform.position, puntoB.position, velocidad * Time.deltaTime);
            transform.position = nuevaPosicion;
        }
        else if (direccionEmpuje == -1)
        {
            // Mover hacia A
            Vector3 nuevaPosicion = Vector3.MoveTowards(transform.position, puntoA.position, velocidad * Time.deltaTime);
            transform.position = nuevaPosicion;
        }
        // Si direccionEmpuje == 0, la plataforma no se mueve
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            PlayerController jugador = collision.gameObject.GetComponent<PlayerController>();
            if (jugador != null)
            {
                jugadorCerca = true;
                jugadorActual = jugador;

                // Buscamos el puntoSuelo del jugador por nombre.
                // Asegúrate que el objeto hijo se llame exactamente "puntoSuelo".
                puntoSueloJugador = jugadorActual.transform.Find("Punto Suelo");
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            PlayerController jugador = collision.gameObject.GetComponent<PlayerController>();
            if (jugador != null && jugador == jugadorActual)
            {
                jugadorCerca = false;
                jugadorActual = null;
                puntoSueloJugador = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(puntoA.position, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(puntoB.position, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(puntoA.position, puntoB.position);
        }
    }
}