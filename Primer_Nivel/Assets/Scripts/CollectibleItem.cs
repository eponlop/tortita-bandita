using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private GameManagerCollectibles gameManager;

    void Start()
    {
        // Intentar encontrar el GameManager al inicio
        gameManager = FindObjectOfType<GameManagerCollectibles>();
        if (gameManager == null)
        {
            Debug.LogError("CollectibleItem requiere que haya un GameManagerCollectibles en la escena.");
        }

        // Asumiendo que el objeto tiene un Collider configurado como Trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto que colisionó es el jugador (usando la etiqueta "Player")
        if (other.CompareTag("Player"))
        {
            if (gameManager != null)
            {
                // 1. Notificar al GameManager que un objeto ha sido recogido
                gameManager.CollectItem();

                // 2. Hacer que el objeto desaparezca
                Destroy(gameObject);
            }
        }
    }
}