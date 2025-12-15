using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPillado : MonoBehaviour
{

    [Header("Referencias de la UI")]
    // Arrastra aquí el Panel que contiene el mensaje y los botones (ej. Panel_Muerte)
    public GameObject indicadorPanel;

    [Header("Configuración de Escenas")]
    // Nombre de la escena del menú principal (ej. "MainMenu")
    public string nombreEscenaMenuPrincipal = "EscenaMenu";

    // Opcional: Referencia a tu PlayerController si implementas un Respawn
    // public PlayerController playerController;
    void Start()
    {
        // 1. Asegura que el panel esté oculto al inicio
        if (indicadorPanel != null)
        {
            indicadorPanel.SetActive(false);
        }

        // 2. Asegura que el juego comience a velocidad normal
        Time.timeScale = 1f;
    }

    public void OnEnemyCaughtPlayer()
    {
        if (indicadorPanel != null)
        {
            Debug.Log("GameManager: Jugador capturado. Activando panel UI.");

            // 1. Muestra el panel de indicación
            indicadorPanel.SetActive(true);

        
        }
        else
        {
            Debug.LogError("FATAL ERROR: El panel indicador no está asignado al GameManager.");
        }
        Time.timeScale = 0f;
    }

    public void GoToMainMenu()
    {
        Debug.Log("Volviendo al menú principal...");

        // Es vital reanudar el tiempo antes de la carga de escena
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nombreEscenaMenuPrincipal))
        {
            SceneManager.LoadScene(nombreEscenaMenuPrincipal);
        }
        else
        {
            Debug.LogError("El nombre de la escena del menú no está configurado.");
        }
    }

    // Método asignado al Botón "Continuar"
    public void ContinueGame()
    {
        Debug.Log("Juego reanudado. Recargando escena por índice.");

        // 1. Obtener el índice de la escena que está cargada actualmente
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 2. Reanudar el juego
        Time.timeScale = 1f;

        // 3. Cargar la escena usando su índice (un 'int' en lugar de un 'string')
        SceneManager.LoadScene(currentSceneIndex);


    }
    // Update is called once per frame
    void Update()
    {

    }

}

