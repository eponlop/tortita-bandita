using UnityEngine;

public class BillboardPlayer : MonoBehaviour
{
    [Header("Referencias")]
    // Referencia al Transform del jugador para saber dónde mirar
    private Transform playerTransform;

    void Start()
    {
        // 1. Encontrar el jugador por Tag al inicio
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("No se encontró un objeto con el tag 'Player'. El Billboard no funcionará.");
            enabled = false; // Desactiva el script si no encuentra al jugador
        }
    }

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            // Crea una copia de la posición del jugador pero usando la altura del ícono
            Vector3 targetPosition = playerTransform.position;
            targetPosition.y = transform.position.y;

            // Mira a esa posición horizontalmente
            transform.LookAt(targetPosition);
        }
    }
}