using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Importante para la Corrutina

public class ExitDoor : MonoBehaviour
{
    [Header("Referencias de la UI de Victoria")]
    public GameObject panelVictoria;

    [Header("Configuración de Escenas")]
    public string nombreEscenaMenu = "EscenaMenu";

    private bool victoriaActivada = false;

    void Start()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si es el jugador y evitar que se repita
        if (other.CompareTag("Player") && !victoriaActivada)
        {
            victoriaActivada = true;

            // --- PASO CLAVE ---
            // Buscamos cualquier componente que pueda estar bloqueando el ratón y lo desactivamos
            // Si usas el Starter Assets o un FPS Controller común, esto suele ser necesario:
            var controller = other.GetComponent<CharacterController>();
            if (controller != null)
            {
                // Si tienes un script específico de cámara o movimiento, desactívalo aquí:
                // other.GetComponent<TuScriptDeMovimiento>().enabled = false;
            }

            StartCoroutine(SecuenciaVictoria());
        }
    }

    private IEnumerator SecuenciaVictoria()
    {
        Debug.Log("Iniciando secuencia de victoria...");

        // 1. Mostrar el panel
        if (panelVictoria != null) panelVictoria.SetActive(true);

        // 2. Liberar el cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 3. ESPERAR UN FRAME (Esto soluciona lo de la 'segunda pasada')
        // Permite que Unity procese la desactivación de otros scripts antes de pausar
        yield return null;

        // 4. Pausar el tiempo
        Time.timeScale = 0f;

        // 5. Reforzar visibilidad (por si el motor lo ocultó al pausar)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

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