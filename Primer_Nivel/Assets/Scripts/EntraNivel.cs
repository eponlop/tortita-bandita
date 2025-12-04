using UnityEngine;
using UnityEngine.SceneManagement;

public class EntraNivel : MonoBehaviour
{
    public string sceneName = "NombreDeLaEscena";

    private void OnTriggerEnter(Collider other)
    {
        // **VERIFICACIÓN CLAVE:** // Comprueba si el objeto que entró en el trigger (other) tiene el tag "Player".
        if (other.CompareTag("Player"))
        {
            // Si tiene el tag "Player", carga la escena.
            SceneManager.LoadScene(sceneName);
        }
    }
}