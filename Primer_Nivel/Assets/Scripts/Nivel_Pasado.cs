using UnityEngine;
using UnityEngine.SceneManagement;

public class Nivel_Pasado : MonoBehaviour
{
    // 1. Asigna el Panel/Canvas que contiene los botones en el Inspector
    public GameObject panelDeOpciones;

    // 2. Nombre de la escena del menú principal (ej: "MenuPrincipal")
    public string nombreEscenaMenu = "EscenaMenu";

    // 3. Nombre de la escena a la que se avanza (ej: "Nivel2")
    public string nombreEscenaSiguienteNivel = "Nivel_1";

    private void Start()
    {
        // Asegúrate de que el panel esté oculto al inicio
        if (panelDeOpciones != null)
        {
            panelDeOpciones.SetActive(false);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Detenemos el tiempo del juego (lo pausamos)
            Time.timeScale = 0f;

            // Mostramos el panel de opciones
            if (panelDeOpciones != null)
            {
                panelDeOpciones.SetActive(true);
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // --- Funciones para los Botones ---

    // Función que se asignará al botón de "Continuar/Siguiente Nivel"
    public void CargarSiguienteNivel()
    {
        // Reanudamos el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaSiguienteNivel);
    }

    // Función que se asignará al botón de "Salir al Menú"
    public void CargarMenuPrincipal()
    {
        // Reanudamos el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}
