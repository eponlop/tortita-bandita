using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private GameManagerCollectibles gameManager;

    void Start()
    {
        // Intentar encontrar el GameManager al inicio
        gameManager = FindObjectOfType<GameManagerCollectibles>();
        if (gameManager == null)
        {
            Debug.LogError("ExitDoor requiere que haya un GameManagerCollectibles en la escena.");
        }

        // Asegúrate de que este objeto tiene un Collider configurado como Trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto que colisionó es el jugador (usando la etiqueta "Player")
        if (other.CompareTag("Player"))
        {
            // La puerta solo debe funcionar si está activa (lo cual controla el GameManager)
            if (gameObject.activeSelf && gameManager != null)
            {
                gameManager.ReachedExit();
            }
        }
    }
}