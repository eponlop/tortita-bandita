using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Nivel_1"); // Cambia "Level1" por el nombre exacto de tu escena
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Nivel_2");
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene("Nivel_3");
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // útil en el editor
    }
}
