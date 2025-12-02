using UnityEngine;
using UnityEngine.UI;

public class AnimacionTextos : MonoBehaviour
{
    public Button toggleButton;     // Asigna el botón en el Inspector
    public Animator textAnimator;   // Asigna el Animator del texto
    private bool isVisible = false; // Estado actual del texto

    void Start()
    {
        // Agrega el listener al botón
        toggleButton.onClick.AddListener(ToggleText);
    }

    void ToggleText()
    {
        isVisible = !isVisible;                    // Cambia el estado
        textAnimator.SetBool("IsVisible", isVisible); // Actualiza el Animator
    }
}