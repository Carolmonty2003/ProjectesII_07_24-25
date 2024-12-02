using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadNormal = 5f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSaltoNormal = 10f;
    [SerializeField] private float gravedadNormal = 2.5f;
    [SerializeField] private float gravedadPlaneo = 0.8f; // Gravedad reducida para planear
    [SerializeField] private float gravedadCrecido = 5f;  // Gravedad aumentada para modo Grow

    [Header("Tamaño del Personaje")]
    [SerializeField] private Vector3 escalaNormal = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 escalaReducido1 = new Vector3(0.75f, 0.75f, 1);
    [SerializeField] private Vector3 escalaReducido2 = new Vector3(0.5f, 0.5f, 1);
    [SerializeField] private Vector3 escalaCrecido = new Vector3(1.5f, 1.5f, 1);

    [Header("Detección de Suelo")]
    [SerializeField] private Transform puntoSuelo;
    [SerializeField] private float radioDeteccion = 0.2f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Partículas")]
    [SerializeField] private ParticleSystem particulasSalto;
    [SerializeField] private ParticleSystem particulasAterrizaje;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // Para voltear el personaje
    private bool puedeSaltar;
    private bool estaEnSuelo;
    private int nivelEncogimiento = 0; // 0: Normal, 1: Reducido1, 2: Reducido2, 3: Grow

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Obtener el SpriteRenderer
    }

    private void Update()
    {
        // Movimiento horizontal
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(inputHorizontal * velocidadNormal, rb.velocity.y);

        // Voltear el sprite según la dirección del movimiento
        if (inputHorizontal != 0)
        {
            spriteRenderer.flipX = inputHorizontal < 0;
            ActualizarDireccionParticulas(); // Ajustar la dirección de las partículas
        }

        // Saltar
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && puedeSaltar)
        {
            Saltar();
        }

        // Ajustar gravedad según el estado
        AjustarGravedad();

        // Verificar si aterrizó
        VerificarSuelo();

        // Escalar partículas
        EscalarParticulas();
    }

    private void VerificarSuelo()
    {
        bool estabaEnSuelo = estaEnSuelo;
        estaEnSuelo = Physics2D.OverlapCircle(puntoSuelo.position, radioDeteccion, capaSuelo);

        if (estaEnSuelo && !estabaEnSuelo)
        {
            // Generar partículas al aterrizar
            if (particulasAterrizaje != null)
            {
                particulasAterrizaje.Play();
            }
        }

        puedeSaltar = estaEnSuelo;
    }

    private void Saltar()
    {
        if (particulasSalto != null)
        {
            particulasSalto.Play(); // Generar partículas al saltar
        }
        rb.velocity = new Vector2(rb.velocity.x, fuerzaSaltoNormal);
    }

    private void AjustarGravedad()
    {
        if (nivelEncogimiento == 3) // Modo Grow: Caída rápida
        {
            rb.gravityScale = gravedadCrecido;
        }
        else if (!estaEnSuelo && rb.velocity.y < 0) // Planeo en el aire
        {
            rb.gravityScale = gravedadPlaneo;
        }
        else
        {
            rb.gravityScale = gravedadNormal; // Estado normal
        }
    }

    private void EscalarParticulas()
    {
        if (particulasSalto != null)
        {
            var main = particulasSalto.main;
            main.startSizeMultiplier = transform.localScale.x; // Escala basada en el tamaño del personaje
        }
        if (particulasAterrizaje != null)
        {
            var main = particulasAterrizaje.main;
            main.startSizeMultiplier = transform.localScale.x; // Escala basada en el tamaño del personaje
        }
    }

    private void ActualizarDireccionParticulas()
    {
        float flipDirection = spriteRenderer.flipX ? -1 : 1;

        if (particulasSalto != null)
        {
            Vector3 particleScale = particulasSalto.transform.localScale;
            particulasSalto.transform.localScale = new Vector3(flipDirection * Mathf.Abs(particleScale.x), particleScale.y, particleScale.z);
        }

        if (particulasAterrizaje != null)
        {
            Vector3 particleScale = particulasAterrizaje.transform.localScale;
            particulasAterrizaje.transform.localScale = new Vector3(flipDirection * Mathf.Abs(particleScale.x), particleScale.y, particleScale.z);
        }
    }

    // Métodos requeridos por el script Boton
    public bool EsGrande()
    {
        return nivelEncogimiento == 3; // Nivel 3 es el modo Grow
    }

    public bool EsNormal()
    {
        return nivelEncogimiento == 0; // Nivel 0 es el estado Normal
    }

    public bool EstaEnNivelDeReduccion(int nivel)
    {
        return nivelEncogimiento == nivel; // Comparar con el nivel requerido
    }

    public void Crecer()
    {
        nivelEncogimiento = 3;
        fuerzaSaltoNormal = 7f; // Cambiamos valores para el modo Grow
        velocidadNormal = 4f;
        CambiarEscala(escalaCrecido);
    }

    public void ReducirANivel1()
    {
        nivelEncogimiento = 1;
        fuerzaSaltoNormal = 12f;
        velocidadNormal = 6f;
        CambiarEscala(escalaReducido1);
    }

    public void ReducirANivel2()
    {
        nivelEncogimiento = 2;
        fuerzaSaltoNormal = 14f;
        velocidadNormal = 8f;
        CambiarEscala(escalaReducido2);
    }

    public void RestablecerEstado()
    {
        nivelEncogimiento = 0;
        fuerzaSaltoNormal = 10f;
        velocidadNormal = 5f;
        CambiarEscala(escalaNormal);
    }

    private void CambiarEscala(Vector3 nuevaEscala)
    {
        transform.localScale = nuevaEscala; // Cambiar tamaño del personaje
    }

    private void OnDrawGizmos()
    {
        // Visualizar el área de detección de suelo en el editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoSuelo.position, radioDeteccion);
    }
}
