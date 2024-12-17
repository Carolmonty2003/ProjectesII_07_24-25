using UnityEngine;
using System.Collections;
 
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadNormal = 5f;
    [SerializeField] private float velocidadReducido1 = 6f;
    [SerializeField] private float velocidadReducido2 = 7f;
    [SerializeField] private float velocidadReducido3 = 8f;
    [SerializeField] private float velocidadCrecido = 4f;
    [SerializeField] private float velocidadCrecido2 = 3f;

    [Header("Salto")]
    [SerializeField] private float alturaSaltoConstante = 3f;
    [SerializeField] private float gravedadNormal = 2.5f;
    [SerializeField] private float gravedadPlaneo = 0.8f;
    [SerializeField] private float gravedadCrecido = 5f;
    [SerializeField] private float gravedadCrecido2 = 6f;

    [Header("Tamaño del Personaje")]
    [SerializeField] private Vector3 escalaNormal = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 escalaReducido1 = new Vector3(0.75f, 0.75f, 1);
    [SerializeField] private Vector3 escalaReducido2 = new Vector3(0.5f, 0.5f, 1);
    [SerializeField] private Vector3 escalaReducido3 = new Vector3(0.25f, 0.25f, 1);
    [SerializeField] private Vector3 escalaCrecido = new Vector3(1.5f, 1.5f, 1);
    [SerializeField] private Vector3 escalaCrecido2 = new Vector3(2, 2, 1);
    [SerializeField] private AnimationCurve curvaDeAnimacion;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform puntoSuelo;
    [SerializeField] private float radioDeteccion = 0.2f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Partículas")]
    [SerializeField] private ParticleSystem particulasSalto;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool puedeSaltar;
    private bool estaEnSuelo;
    private int nivelEncogimiento = 0;
    private bool bloqueadoPorAnimacion = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (bloqueadoPorAnimacion)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(inputHorizontal * ObtenerVelocidad(), rb.velocity.y);

        if (inputHorizontal != 0)
        {
            spriteRenderer.flipX = inputHorizontal < 0;
        }

        // Salto con teclas W, Espacio, Flecha Arriba y botón X del mando
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetButtonDown("Jump")) && puedeSaltar)
        {
            Saltar();
        }

        AjustarGravedad();
        VerificarSuelo();
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

        float alturaDesdePies = alturaSaltoConstante + transform.localScale.y / 2;
        float fuerzaSalto = Mathf.Sqrt(2 * alturaDesdePies * gravedadNormal);

        rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);

        PlataformaToggle.AlternarGrupos();
    }

    private void AjustarGravedad()
    {
        if (nivelEncogimiento == 5)
        {
            rb.gravityScale = gravedadCrecido2;
        }
        else if (nivelEncogimiento == 4)
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
            5 => velocidadCrecido2,
            _ => velocidadNormal,
        };
    }

    private void ActualizarAnimaciones(float inputHorizontal)
    {
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
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsGliding", false);
            }
            else if (rb.velocity.y < 0)
            {
                animator.SetBool("IsJumping", false);

                if (nivelEncogimiento == 4 || nivelEncogimiento == 5)
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
    }

    public bool EsGrande()
    {
        return nivelEncogimiento == 4 || nivelEncogimiento == 5;
    }

    public bool EstaEnNivelDeReduccion(int nivel)
    {
        return nivelEncogimiento == nivel;
    }

    public void ReducirANivel1()
    {
        if (nivelEncogimiento == 0 || nivelEncogimiento == 2)
        {
            nivelEncogimiento = 1;
            CambiarEscala(escalaReducido1);
        }
    }

    public void ReducirANivel2()
    {
        if (nivelEncogimiento == 1)
        {
            nivelEncogimiento = 2;
            CambiarEscala(escalaReducido2);
        }
    }

    public void ReducirANivel3()
    {
        if (nivelEncogimiento == 2)
        {
            nivelEncogimiento = 3;
            CambiarEscala(escalaReducido3);
        }
    }

    public void Crecer()
    {
        nivelEncogimiento = 4;
        CambiarEscala(escalaCrecido);
    }

    public void CrecerANivel2()
    {
        nivelEncogimiento = 5;
        CambiarEscala(escalaCrecido2);
    }

    public void RestablecerEstado()
    {
        nivelEncogimiento = 0;
        CambiarEscala(escalaNormal);
    }

    public void RestablecerDesdeCrecido2()
    {
        if (nivelEncogimiento == 5)
        {
            nivelEncogimiento = 0;
            CambiarEscala(escalaNormal);
        }
    }

    private void CambiarEscala(Vector3 nuevaEscala)
    {
        float duracionAnimacion = 0.5f;
        StartCoroutine(AnimarCambioDeTamaño(nuevaEscala, duracionAnimacion));
    }

    private IEnumerator AnimarCambioDeTamaño(Vector3 escalaObjetivo, float duracion)
    {
        bloqueadoPorAnimacion = true;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsFalling", false);
        animator.SetBool("IsGliding", false);

        Vector3 escalaInicial = transform.localScale;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = curvaDeAnimacion.Evaluate(tiempo / duracion);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaObjetivo, progreso);
            yield return null;
        }

        transform.localScale = escalaObjetivo;
        bloqueadoPorAnimacion = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoSuelo.position, radioDeteccion);
    }

    public bool EresMuyGrande()
    {
        if (nivelEncogimiento == 5)
            return true;
        else
            return false;
    }
}
