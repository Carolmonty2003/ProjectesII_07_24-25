using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoodbyeBuddy
{
    public class WaveScript : MonoBehaviour
    {
        public Transform playerTransform;

        public float speed = 5f;
        public float chaseRange = 10f; // Distancia para iniciar la persecución
        public float retreatRange = 3f; // Distancia para retroceder
        public float idleSpeed = 2f; // Velocidad cuando está patrullando
        public Transform[] patrolPoints; // Puntos de patrulla
        private int currentPatrolIndex = 0;
        private enum WaveState { Idle, Chasing, Retreating }
        private WaveState currentState = WaveState.Idle;

        private void Update()
        {
            // Detectar al jugador dinámicamente si aún no se ha encontrado
            if (playerTransform == null)
            {
                DetectPlayer();
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Cambiar estados según la distancia al jugador
            if (distanceToPlayer < chaseRange && distanceToPlayer > retreatRange)
            {
                currentState = WaveState.Chasing;
            }
            else if (distanceToPlayer <= retreatRange)
            {
                currentState = WaveState.Retreating;
            }
            else
            {
                currentState = WaveState.Idle;
            }

            // Lógica de estados
            switch (currentState)
            {
                case WaveState.Idle:
                    Patrol();
                    break;
                case WaveState.Chasing:
                    ChasePlayer();
                    break;
                case WaveState.Retreating:
                    Retreat();
                    break;
            }
        }

        private void Patrol()
        {
            if (patrolPoints.Length == 0)
            {
                Debug.Log("No hay puntos de patrulla asignados.");
                return;
            }

            if (patrolPoints.Length == 0) return;

            Transform targetPoint = patrolPoints[currentPatrolIndex];
            transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, idleSpeed * Time.deltaTime);
            
            Debug.Log($"Moviendo hacia el punto {currentPatrolIndex}");
            
            if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }

        private void ChasePlayer()
        {
            if (playerTransform != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
            }
        }

        private void Retreat()
        {
            if (playerTransform != null)
            {
                Vector2 retreatDirection = (transform.position - playerTransform.position).normalized;
                transform.position += (Vector3)(retreatDirection * speed * Time.deltaTime);
            }
        }

        private void DetectPlayer()
        {
            // Usamos OverlapCircle para detectar al jugador en un rango
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, chaseRange, LayerMask.GetMask("Player"));
            if (playerCollider != null && playerCollider.TryGetComponent(out PlayerController player))
            {
                playerTransform = player.transform;
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.CompareTag("Player"))
            {
                Debug.Log("Player Died");
                collision.transform.GetComponent<PlayerRespawn>().PlayerDied();
            }
        }
    }
}
