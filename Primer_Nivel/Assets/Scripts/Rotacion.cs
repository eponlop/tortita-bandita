using UnityEngine;
using UnityEngine.InputSystem;

public class RotateChildRelative : MonoBehaviour
{
    [Header("Player Dependencies")]
    // Referencia al script que maneja la Stamina
    public PlayerController playerController;

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

        if (playerController == null)
        {
            //Debug.LogError("PlayerController no está asignado en RotateChildRelative. La rotación no verificará la Stamina.");
            playerController = GetComponentInParent<PlayerController>();
        }
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
        bool canRotate = true;

        // 1. Verificar si tenemos el PlayerController y si se cumplen las condiciones de INICIO
        if (playerController != null)
        {
            // La Stamina debe ser suficiente (>= 25%) y no debe estar cansado para *iniciar* la rotación.
            float minStamina = playerController.minStaminaToRun;

            if (playerController.isTired || playerController.currentStamina < minStamina)
            {
                canRotate = false;
                Debug.Log("Rotación bloqueada: No se cumplen las condiciones de inicio.");
            }
        }

        if (canRotate)
        {
            isRotating = true;
            // Calculamos la rotación relativa al padre
            targetLocalRotation = originalLocalRotation * Quaternion.Euler(0f, rotationAngle, 0f);
            Debug.Log("Rotación iniciada: Tecla pulsada");
        }
        else
        {
            isRotating = false;
        }
    }

    private void OnKeyReleased(InputAction.CallbackContext context)
    {
        isRotating = false;
        Debug.Log("Tecla liberada");
    }

    void Update()
    {
        // 1. **VERIFICACIÓN CONSTANTE DE CANCELACIÓN (Solo al estar cansado)**
        // Esto permite que la acción continúe hasta que se alcance el agotamiento total (0%).
        if (isRotating && playerController != null)
        {
            // Condición de cancelación: Solo si el jugador está completamente cansado (isTired = true).
            if (playerController.isTired)
            {
                isRotating = false;
                Debug.Log("Rotación cancelada: El personaje está completamente agotado (isTired).");
            }
        }

        // 2. Interpolación 
        childToRotate.localRotation = Quaternion.Slerp(
            childToRotate.localRotation,
            isRotating ? targetLocalRotation : originalLocalRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}