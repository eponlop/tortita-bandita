using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Deformarse : MonoBehaviour
{

    private CharacterController controller;
    // Referencia al Input Action "Deformarse"
    public InputActionReference deformAction;

    // Escala original y escala objetivo
    private Vector3 originalScale;
    public Vector3 targetScale = new Vector3(2f, 2f, 2f);

    void Start()
    {
        originalScale = transform.localScale;

        if (deformAction != null)
        {
            deformAction.action.performed += OnDeformarse;
        }
    }

    private void OnDeformarse(InputAction.CallbackContext context)
    {
        // Alterna entre escala original y objetivo
        if (transform.localScale == originalScale)
            transform.localScale = targetScale;
        else
            transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (deformAction != null)
        {
            deformAction.action.performed -= OnDeformarse;
        }
    }
}
