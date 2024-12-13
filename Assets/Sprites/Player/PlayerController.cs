using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadNormal = 5f;
    [SerializeField] private float velocidadReducido1 = 6f;
    [SerializeField] private float velocidadReducido2 = 7f;
    [SerializeField] private float velocidadReducido3 = 8f;
    [SerializeField] private float velocidadCrecido = 4f;

    [Header("Salto")]
    [SerializeField] private float alturaSaltoConstante = 3f; // Altura medida desde los pies
    [SerializeField] private float gravedadNormal = 2.5f;
    [SerializeField] private float gravedadPlaneo = 0.8f;
    [SerializeField] private float gravedadCrecido = 5f;

    [Header("Tamaño del Personaje")]
    [SerializeField] private Vector3 escalaNormal = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 escalaReducido1 = new Vector3(0.75f, 0.75f, 1);
    [SerializeField] private Vector3 escalaReducido2 = new Vector3(0.5f, 0.5f, 1);
    [SerializeField] private Vector3 escalaReducido3 = new Vector3(0.25f, 0.25f, 1);
    [SerializeField] private Vector3 escalaCrecido = new Vector3(1.5f, 1.5f, 1);

    [Header("Detección de Suelo")]
    [SerializeField] private Transform puntoSuelo;
    [SerializeField] private float radioDeteccion = 0.2f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Partículas")]
    [SerializeField] private ParticleSystem particulasSalto;
    [SerializeField] private ParticleSystem particulasAterrizaje;

    [Header("Animator")]
    [SerializeField] private Animator animator; // Referencia al Animator

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool puedeSaltar;
    private bool estaEnSuelo;
    private int nivelEncogimiento = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(inputHorizontal * ObtenerVelocidad(), rb.velocity.y);

        if (inputHorizontal != 0)
        {
            spriteRenderer.flipX = inputHorizontal < 0;
        }

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && puedeSaltar)
        {
            Saltar();
        }

        AjustarGravedad();
        VerificarSuelo();

        // Actualizar animación
        ActualizarAnimaciones(inputHorizontal);
    }

    private void VerificarSuelo()
    {
        estaEnSuelo = Physics2D.OverlapCircle(puntoSuelo.position, radioDeteccion, capaSuelo);
        puedeSaltar = estaEnSuelo;
    }

    private void Saltar()
    {
        if (particulasSalto != null)
        {
            particulasSalto.Play();
        }

        float alturaDesdePies = alturaSaltoConstante + transform.localScale.y / 2; // Ajusta por la mitad de la altura del personaje
        float fuerzaSalto = Mathf.Sqrt(2 * alturaDesdePies * gravedadNormal);

        rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);

        PlataformaToggle.AlternarGrupos(); // Asegúrate de que este método exista y sea relevante.
    }

    private void AjustarGravedad()
    {
        if (nivelEncogimiento == 4)
        {
            rb.gravityScale = gravedadCrecido;
        }
        else if (!estaEnSuelo && rb.velocity.y < 0)
        {
            rb.gravityScale = gravedadPlaneo;
        }
        else
        {
            rb.gravityScale = gravedadNormal;
        }
    }

    private float ObtenerVelocidad()
    {
        return nivelEncogimiento switch
        {
            1 => velocidadReducido1,
            2 => velocidadReducido2,
            3 => velocidadReducido3,
            4 => velocidadCrecido,
            _ => velocidadNormal,
        };
    }

    private void ActualizarAnimaciones(float inputHorizontal)
    {
        // WALK/IDLE
        if (estaEnSuelo)
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsGliding", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsWalking", inputHorizontal != 0);
        }
        else
        {
            animator.SetBool("IsWalking", false);

            if (rb.velocity.y > 0)
            {
                // JUMP
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsGliding", false);
            }
            else if (rb.velocity.y < 0)
            {
                animator.SetBool("IsJumping", false);

                // FALL vs GLIDE
                if (nivelEncogimiento == 4)
                {
                    animator.SetBool("IsFalling", true);
                    animator.SetBool("IsGliding", false);
                }
                else
                {
                    animator.SetBool("IsFalling", false);
                    animator.SetBool("IsGliding", true);
                }
            }
        }

        // IDLE
        if (estaEnSuelo && inputHorizontal == 0)
        {
            animator.SetBool("IsWalking", false);
        }
    }

    public bool EsGrande()
    {
        return nivelEncogimiento == 4;
    }

    public bool EsNormal()
    {
        return nivelEncogimiento == 0;
    }

    public bool EstaEnNivelDeReduccion(int nivel)
    {
        return nivelEncogimiento == nivel;
    }

    public void Crecer()
    {
        nivelEncogimiento = 4;
        CambiarEscala(escalaCrecido);
    }

    public void ReducirANivel1()
    {
        nivelEncogimiento = 1;
        CambiarEscala(escalaReducido1);
    }

    public void ReducirANivel2()
    {
        nivelEncogimiento = 2;
        CambiarEscala(escalaReducido2);
    }

    public void ReducirANivel3()
    {
        nivelEncogimiento = 3;
        CambiarEscala(escalaReducido3);
    }

    public void RestablecerEstado()
    {
        nivelEncogimiento = 0;
        CambiarEscala(escalaNormal);
    }

    private void CambiarEscala(Vector3 nuevaEscala)
    {
        transform.localScale = nuevaEscala;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoSuelo.position, radioDeteccion);
    }
}
