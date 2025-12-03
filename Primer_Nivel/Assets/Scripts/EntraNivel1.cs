using UnityEngine;
using UnityEngine.SceneManagement;

public class EntraNivel1 : MonoBehaviour
{
    public string sceneName = "NombreDeLaEscena";

    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(sceneName);
    }
}