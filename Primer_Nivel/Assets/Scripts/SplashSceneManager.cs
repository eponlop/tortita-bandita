using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SplashSceneManager : MonoBehaviour
{
    public float waitTime = 10.0f;
    void Start()
    {
        StartCoroutine(WaitAndLoadNextLevel());
    }

    private void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame)
        {
            LoadNextLevel();
        }
    }
    IEnumerator WaitAndLoadNextLevel()
    {
        yield return new WaitForSeconds(waitTime);
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene("EscenaMenu");
    }
}
