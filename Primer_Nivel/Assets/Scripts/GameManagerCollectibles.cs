using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar/cambiar escenas

public class GameManagerCollectibles : MonoBehaviour
{
    [Header("Objetivos del Juego")]
    [Tooltip("Número total de objetos que el jugador debe recoger.")]
    public int totalCollectibles = 3;

    [Tooltip("El objeto de la salida que se activa al completar la recogida.")]
    public GameObject exitDoor;

    private int collectiblesCollected = 0;

    void Start()
    {
        // Asegurarse de que la puerta esté inicialmente inactiva o cerrada
        if (exitDoor != null)
        {
            exitDoor.SetActive(false);
        }
    }

    /// <summary>
    /// Llamado por los objetos coleccionables cuando el jugador los toca.
    /// </summary>
    public void CollectItem()
    {
        collectiblesCollected++;
        Debug.Log($"Objeto recogido. Total: {collectiblesCollected} / {totalCollectibles}");

        // Comprobar si se han recogido todos los objetos
        if (collectiblesCollected >= totalCollectibles)
        {
            ObjectivesComplete();
        }
    }

    /// <summary>
    /// Se llama cuando se han recogido todos los objetos.
    /// </summary>
    private void ObjectivesComplete()
    {
        Debug.Log("¡Objetivos de recogida completados! La salida está abierta.");

        if (exitDoor != null)
        {
            // Activamos visualmente la salida (por ejemplo, encendiendo la luz o haciéndola visible)
            exitDoor.SetActive(true);
            // O, si la puerta tiene un componente de renderizado/colisionador que ya está activo
            // y solo quieres cambiar el aspecto visual:
            // Renderer doorRenderer = exitDoor.GetComponent<Renderer>();
            // if(doorRenderer != null) doorRenderer.material.color = Color.green;
        }
    }
}