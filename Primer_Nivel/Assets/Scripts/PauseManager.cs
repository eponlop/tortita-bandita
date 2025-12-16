using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;
    public GameObject opcionesMenuUI;

    private bool isPaused = false;
    private Coroutine cursorFixCoroutine;

    void Start()
    {
        HideAndLockCursor();

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    // ESTE ES EL NUEVO MÉTODO QUE USA EL NUEVO INPUT SYSTEM
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        //Debug.Log("PAUSE PRESSED");
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cursorFixCoroutine != null) StopCoroutine(cursorFixCoroutine);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        opcionesMenuUI.SetActive(false);

        if (cursorFixCoroutine != null) StopCoroutine(cursorFixCoroutine);
        cursorFixCoroutine = StartCoroutine(FixCursorNextFrame());
    }

    IEnumerator FixCursorNextFrame()
    {
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
        SceneManager.LoadScene("EscenaMenu");
    }
}
