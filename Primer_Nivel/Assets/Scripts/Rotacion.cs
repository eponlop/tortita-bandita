using UnityEngine;
using UnityEngine.InputSystem;

public class RotateChildRelative : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference rotateAction; // la tecla para girar

    [Header("Target")]
    public Transform childToRotate; // el hijo que quieres girar

    [Header("Rotación")]
    public float rotationAngle = 90f;   // ángulo relativo al padre
    public float rotationSpeed = 5f;    // suavidad de la transición

    private bool isRotating = false;
    private Quaternion originalLocalRotation;
    private Quaternion targetLocalRotation;

    void Start()
    {
        if (childToRotate == null)
            childToRotate = transform;

        // Guardamos la rotación local original del hijo
        originalLocalRotation = childToRotate.localRotation;
    }

    void OnEnable()
    {
        if (rotateAction != null)
        {
            rotateAction.action.Enable();
            rotateAction.action.performed += OnKeyPressed;
            rotateAction.action.canceled += OnKeyReleased;
        }
    }

    void OnDisable()
    {
        if (rotateAction != null)
        {
            rotateAction.action.performed -= OnKeyPressed;
            rotateAction.action.canceled -= OnKeyReleased;
            rotateAction.action.Disable();
        }
    }

    private void OnKeyPressed(InputAction.CallbackContext context)
    {
        isRotating = true;
        // Calculamos la rotación relativa al padre cuando se pulsa la tecla
        targetLocalRotation = originalLocalRotation * Quaternion.Euler(0f, rotationAngle, 0f);
        Debug.Log("Tecla pulsada");
    }

    private void OnKeyReleased(InputAction.CallbackContext context)
    {
        isRotating = false;
        Debug.Log("Tecla liberada");
    }

    void Update()
    {
        // Interpolamos suavemente entre la rotación original y la rotación relativa
        childToRotate.localRotation = Quaternion.Slerp(
            childToRotate.localRotation,
            isRotating ? targetLocalRotation : originalLocalRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
