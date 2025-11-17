using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI; // Arrastra aquí tu panel de pausa

    private bool isPaused = false;
    private Coroutine cursorFixCoroutine;
    

    void Start()
    {
        // Estado inicial: juego funcionando, cursor oculto y bloqueado
        HideAndLockCursor();

        // Asegúrate de que el menú de pausa empieza desactivado
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        // Mostrar cursor para usar el menú
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Cancelar corrutinas pendientes
        if (cursorFixCoroutine != null) StopCoroutine(cursorFixCoroutine);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        // Aplicar bloqueo del cursor en el siguiente frame
        if (cursorFixCoroutine != null) StopCoroutine(cursorFixCoroutine);
        cursorFixCoroutine = StartCoroutine(FixCursorNextFrame());
    }

    IEnumerator FixCursorNextFrame()
    {
        // Esperar un frame porque el UI o el EventSystem pueden modificar el cursor
        yield return null;
        HideAndLockCursor();
        cursorFixCoroutine = null;
    }

    private void HideAndLockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        if (cursorFixCoroutine != null) { StopCoroutine(cursorFixCoroutine); cursorFixCoroutine = null; }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        if (cursorFixCoroutine != null) { StopCoroutine(cursorFixCoroutine); cursorFixCoroutine = null; }
        SceneManager.LoadScene("MainMenu");
    }
}
