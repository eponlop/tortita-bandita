using UnityEngine;

public class SiropeEffect : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [Tooltip("Tiempo en segundos antes de que la mancha se destruya.")]
    public float lifetime = 10f;

    [Tooltip("Factor de ralentización (1.0 = sin efecto, 0.5 = 50% de velocidad).")]
    public float allySlowFactor = 0.5f;
    public float enemySlowFactor = 0.1f;

    [Header("Objetivos de Ralentización")]
    [Tooltip("Etiquetas (Tags) de los objetos que deben ser ralentizados (ej: 'Enemy', 'Player').")]
    // Se recomienda que los enemigos tengan el tag "Enemy" y el aliado el tag "Player"
    public string enemyTag = "Enemy";
    public string allyTag = "Player";


    private void Start()
    {
        // 1. Destrucción Temporizada
        // Inicia la cuenta regresiva. Después de 'lifetime' segundos, el objeto se destruye.
        Destroy(gameObject, lifetime);
    }

    // =========================================================================
    // ⚙️ LÓGICA DE INTERACCIÓN: ENTRADA Y SALIDA DEL ÁREA
    // =========================================================================

    // Se llama cuando otro Collider entra en el trigger de la mancha de sirope
    private void OnTriggerEnter(Collider other)
    {
        // Intentamos obtener el componente del aliado (PlayerController)
        if (other.CompareTag(allyTag))
        {
            PlayerController allyController = other.GetComponent<PlayerController>();

            if (allyController != null)
            {
                // Aplicamos el método ApplySlow del script PlayerController
                allyController.ApplySlow(allySlowFactor);
                Debug.Log($"Ralentizando al aliado ({other.name}) con un factor de {allySlowFactor}.");
            }
        }
        // Intentamos obtener el componente del enemigo (WanderingAI)
        else if (other.CompareTag(enemyTag))
        {
            // ⭐ Nota: El script del enemigo es WanderingAI, no EnemyMovement
            WanderingAI enemyController = other.GetComponent<WanderingAI>();

            if (enemyController != null)
            {
                // Aplicamos el método ApplySlow del script WanderingAI
                enemyController.ApplySlow(enemySlowFactor);
                Debug.Log($"Ralentizando al enemigo ({other.name}) con un factor de {enemySlowFactor}.");
            }
        }
    }

    // Se llama cuando otro Collider sale del trigger de la mancha de sirope
    private void OnTriggerExit(Collider other)
    {
        // Intentamos obtener el componente del aliado (PlayerController)
        if (other.CompareTag(allyTag))
        {
            PlayerController allyController = other.GetComponent<PlayerController>();

            if (allyController != null)
            {
                // Aplicamos el método RemoveSlow del script PlayerController
                allyController.RemoveSlow();
                Debug.Log($"Removiendo la ralentización del aliado ({other.name}).");
            }
        }
        // Intentamos obtener el componente del enemigo (WanderingAI)
        else if (other.CompareTag(enemyTag))
        {
            WanderingAI enemyController = other.GetComponent<WanderingAI>();

            if (enemyController != null)
            {
                // Aplicamos el método RemoveSlow del script WanderingAI
                enemyController.RemoveSlow();
                Debug.Log($"Removiendo la ralentización del enemigo ({other.name}).");
            }
        }
    }
}