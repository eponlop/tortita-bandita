using UnityEngine;
using UnityEngine.SceneManagement; // necesario para cambiar de escena
using UnityEngine.UI;

public class CambiarEscena : MonoBehaviour
{
    public Button miBoton;      // Asigna el botón en el Inspector
    public string nombreEscena; // Nombre exacto de la escena a cargar

    void Start()
    {
        miBoton.onClick.AddListener(() => SceneManager.LoadScene(nombreEscena));
    }
}
