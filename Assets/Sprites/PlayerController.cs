using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadNormal = 5f;
    public float velocidadAumentada = 3f;  // Velocidad cuando el personaje crece
    public float velocidadReducida1 = 7f; // Velocidad tras el primer encogimiento
    public float velocidadReducida2 = 9f; // Velocidad tras el segundo encogimiento
    public float fuerzaSaltoNormal = 10f;
    public float fuerzaSaltoAumentada = 7f; // Menor salto cuando crece
    public float fuerzaSaltoReducida1 = 12f; // Mayor salto tras el primer encogimiento
    public float fuerzaSaltoReducida2 = 14f; // Mayor salto tras el segundo encogimiento

    [Header("Tamaño del Personaje")]
    public Vector3 tamañoNormal = Vector3.one;
    public Vector3 tamañoAumentado = new Vector3(2f, 2f, 1f); // Crecido
    public Vector3 tamañoReducido1 = new Vector3(0.75f, 0.75f, 1f); // Primer encogimiento
    public Vector3 tamañoReducido2 = new Vector3(0.5f, 0.5f, 1f); // Segundo encogimiento

    [Header("Peso del Personaje")]
    public float masaNormal = 1f;
    public float masaAumentada = 3f;   // Peso al crecer
    public float masaReducida1 = 0.7f; // Peso tras el primer encogimiento
    public float masaReducida2 = 0.5f; // Peso tras el segundo encogimiento

    private Rigidbody2D rb;
    private bool puedeSaltar = false;
    private float velocidadActual;
    private float fuerzaSaltoActual;

    // Estado de encogimiento
    private int nivelEncogimiento = 0; // 0 = Normal, 1 = Reducido una vez, 2 = Reducido dos veces

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        RestablecerEstado(); // Asegura que el personaje comience en su estado normal
    }

    private void Update()
    {
        // Movimiento lateral
        float movimiento = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(movimiento * velocidadActual, rb.velocity.y);

        // Verificar salto
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && puedeSaltar)
        {
            rb.velocity = new Vector2(rb.velocity.x, fuerzaSaltoActual);
            puedeSaltar = false;
        }

        // Flip del personaje (gira según dirección de movimiento)
        if (movimiento != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movimiento) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            puedeSaltar = true;
        }
    }

    // Cambia al tamaño aumentado
    public void Crecer()
    {
        transform.localScale = tamañoAumentado;
        velocidadActual = velocidadAumentada;
        fuerzaSaltoActual = fuerzaSaltoAumentada;
        rb.mass = masaAumentada; // Aumenta la masa del Rigidbody
        nivelEncogimiento = 0; // Reset del encogimiento al crecer
    }

    // Reduce el tamaño al nivel 1
    public void ReducirANivel1()
    {
        if (nivelEncogimiento == 0) // Solo si está en estado normal
        {
            transform.localScale = tamañoReducido1;
            velocidadActual = velocidadReducida1;
            fuerzaSaltoActual = fuerzaSaltoReducida1;
            rb.mass = masaReducida1; // Reduce la masa del Rigidbody
            nivelEncogimiento = 1;
        }
    }

    // Reduce el tamaño al nivel 2
    public void ReducirANivel2()
    {
        if (nivelEncogimiento == 1) // Solo si ya está en nivel 1
        {
            transform.localScale = tamañoReducido2;
            velocidadActual = velocidadReducida2;
            fuerzaSaltoActual = fuerzaSaltoReducida2;
            rb.mass = masaReducida2; // Reduce aún más la masa
            nivelEncogimiento = 2;
        }
    }

    // Restaura al estado normal
    public void RestablecerEstado()
    {
        // Restablece el tamaño, velocidad, salto y masa
        transform.localScale = tamañoNormal;
        velocidadActual = velocidadNormal;
        fuerzaSaltoActual = fuerzaSaltoNormal;
        rb.mass = masaNormal;

        // Reinicia el estado de encogimiento
        nivelEncogimiento = 0;
    }

    // Verifica si el personaje está en su tamaño original
    public bool EsNormal()
    {
        return nivelEncogimiento == 0 && transform.localScale == tamañoNormal;
    }

    // Verifica si el personaje está en su tamaño grande
    public bool EsGrande()
    {
        return transform.localScale == tamañoAumentado;
    }

    // Verifica si está en un nivel específico de encogimiento
    public bool EstaEnNivelDeReduccion(int nivel)
    {
        return nivelEncogimiento == nivel;
    }
}
