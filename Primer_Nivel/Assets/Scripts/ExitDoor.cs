using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class ExitDoor : MonoBehaviour
{
    [Header("Referencias de la UI de Victoria")]
    // Arrastra aquí el Panel que dice "¡Te has pasado el juego!"
    public GameObject panelVictoria;

    [Header("Configuración de Escenas")]
    public string nombreEscenaMenu = "EscenaMenu";

    private GameManagerCollectibles gameManagerCollectibles;

    void Start()
    {
        // Buscamos el gestor de coleccionables si existe
        gameManagerCollectibles = FindObjectOfType<GameManagerCollectibles>();

        // Aseguramos que el panel de victoria esté oculto al empezar
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Detectar si es el jugador
        if (other.CompareTag("Player"))
        {
            // 2. Ejecutar la lógica de victoria
            ActivarVictoria();

            // 3. Avisar al sistema de coleccionables (si existe)
            if (gameManagerCollectibles != null)
            {
                gameManagerCollectibles.ReachedExit();
            }
        }
    }

    private void ActivarVictoria()
    {
        if (panelVictoria != null)
        {
            Debug.Log("¡Victoria! Mostrando panel.");

            // Mostrar el cartel de victoria
            panelVictoria.SetActive(true);

            // Hacer visible el cursor y liberarlo
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Detener el tiempo del juego
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("No has asignado el Panel de Victoria al script ExitDoor.");
        }
    }

    // --- MÉTODOS PARA LOS BOTONES DEL PANEL DE VICTORIA ---

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}