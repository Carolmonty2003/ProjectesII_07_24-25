using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlataform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;       // Velocidad de movimiento
    public float moveDistance = 5f;   // Distancia que recorre hacia arriba y abajo

    private Vector3 startPosition;    // Posición inicial de la plataforma
    private bool movingUp = true;     // Indica si la plataforma se está moviendo hacia arriba

    void Start()
    {
        // Guardar la posición inicial
        startPosition = transform.position;
    }

    void Update()
    {
        // Determinar dirección del movimiento
        float direction = movingUp ? 1f : -1f;

        // Mover la plataforma
        transform.position += Vector3.up * direction * moveSpeed * Time.deltaTime;

        // Verificar si se alcanzó el límite superior o inferior
        if (movingUp && transform.position.y >= startPosition.y + moveDistance)
        {
            movingUp = false; // Cambiar dirección hacia abajo
        }
        else if (!movingUp && transform.position.y <= startPosition.y)
        {
            movingUp = true; // Cambiar dirección hacia arriba
        }
    }
}
