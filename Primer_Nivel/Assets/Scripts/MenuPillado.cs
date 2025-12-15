using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPillado : MonoBehaviour
{

    [Header("Referencias de la UI")]
    public GameObject indicadorPanel;

    [Header("Configuración de Escenas")]
    public string nombreEscenaMenuPrincipal = "EscenaMenu";

    // Coroutine para la corrección del cursor si es necesario, aunque no siempre se requiere
    // private Coroutine cursorFixCoroutine;

    void Start()
    {
        // Aseguramos que el cursor esté oculto y bloqueado al inicio del juego normal.
        HideAndLockCursor();

        if (indicadorPanel != null)
        {
            indicadorPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    // Función auxiliar para ocultar/bloquear el cursor (estado normal de juego)
    private void HideAndLockCursor()
    {
        // Si no queremos que el jugador vea el cursor en el juego normal:
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnEnemyCaughtPlayer()
    {
        Cursor.visible = true;
        if (indicadorPanel != null)
        {
            Debug.Log("GameManager: Jugador capturado. Activando panel UI.");

            // 1. Muestra el panel de indicación
            indicadorPanel.SetActive(true);

            // 2. Muestra y libera el cursor para que el jugador pueda interactuar con el menú
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Debug.LogError("FATAL ERROR: El panel indicador no está asignado al GameManager.");
        }
        // 3. Detiene el tiempo
        Time.timeScale = 0f;
    }

    public void GoToMainMenu()
    {
        Debug.Log("Volviendo al menú principal...");
        // Restaurar tiempo (importante para que la próxima escena cargue)
        Time.timeScale = 1f;
        // Ocultar/Bloquear cursor antes de cambiar de escena (opcional, pero buena práctica)
        HideAndLockCursor();
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }

    public void ContinueGame()
    {
        Debug.Log("Juego reanudado. Recargando escena por índice.");
        // Restaurar tiempo
        Time.timeScale = 1f;
        // Ocultar/Bloquear cursor para volver al juego normal (lo haría Start() si recargas)
        HideAndLockCursor();
        SceneManager.LoadScene("Tutorial");
    }

    // Update is called once per frame
    void Update()
    {
    }

}