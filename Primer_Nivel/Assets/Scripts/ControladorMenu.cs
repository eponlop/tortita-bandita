using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Añadido por si quieres cargar la escena de menú aquí

public class ControladorMenu : MonoBehaviour
{
    [Header("Configuración de Carga")]
    public float waitTime = 4.0f;
    public Animator[] animators; // Lista de animators

    // Opcional: Nombre de la escena a cargar si este script NO está en la escena de menú
    // public string menuSceneName = "MenuPrincipal"; 

    void Start()
    {
        // 1. Asegurar que el cursor esté visible y desbloqueado inmediatamente al inicio.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(WaitAndLoadMenu());
    }

    IEnumerator WaitAndLoadMenu()
    {
        yield return new WaitForSeconds(waitTime);
        LoadMenu();
    }

    private void LoadMenu()
    {
        // 2. Repetir la configuración del cursor para asegurar que permanezca visible.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Activa el trigger en cada Animator de la lista
        foreach (Animator anim in animators)
        {
            if (anim != null)
            {
                anim.SetTrigger("Inicio");
            }
        }

        // --- Si este script debe cargar la escena de menú después de la intro: ---
        // SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Esto solo funciona en el editor de Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Esto funciona en el juego compilado (build)
        Application.Quit();
#endif
    }
}