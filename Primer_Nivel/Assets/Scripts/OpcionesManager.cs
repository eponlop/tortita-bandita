using UnityEngine;
using UnityEngine.SceneManagement; // Necesario si vas a cargar escenas (Jugar)

public class OpcionesMager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject optionsPanel;

    // Se llama al pulsar el botón "Opciones"
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);  // Muestra las opciones
    }

    // Se llama al pulsar el botón "Volver" dentro de opciones
    public void CloseOptions()
    {
        optionsPanel.SetActive(false); // Oculta las opciones
    }
}