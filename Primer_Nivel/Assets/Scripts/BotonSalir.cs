using UnityEngine;
using UnityEngine.UI;

public class BotonSalir : MonoBehaviour
{
    public Button miBoton; // Asigna tu botón en el Inspector

    void Start()
    {
        miBoton.onClick.AddListener(SalirDelJuego);
    }

    void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit(); // Cierra la aplicación compilada
    }
}
