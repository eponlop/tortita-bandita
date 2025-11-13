using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    private float currentSpeed;
    private bool isRunning;

    public float maxStamina = 100f;             // Resistencia máxima
    public float currentStamina;                // Resistencia actual
    public float staminaDrain = 20f;            // Por segundo al correr
    public float staminaRecovery = 10f;         // Por segundo al no correr
    public float recoveryDelay = 2f;            // Segundos antes de empezar a recuperar
    public float recoveryTimer = 0f;            // Temporizador de recuperación
    public float staminaRecoveryStill = 5f;     // Por segundo al estar quieto
    public float minStaminaToRun = 25f;         // Resistencia mínima para poder correr
    public bool isTired = false;


    private CharacterController controller;
    [SerializeField] private Transform camera;
    public float rotSpeed = 15.0f;
    private Vector2 moveInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        currentStamina = maxStamina;
    }

    // Movimiento (Input del joystick o teclado)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Correr (Input de botón)
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed && currentStamina >= minStaminaToRun)
        {
            isRunning = true;
            Debug.Log("Running");
        }

        else if (context.canceled)
        {
            isRunning = false;
            Debug.Log("BackToWalk");
        }
    }

    private void Update()
    {
        // Selecciona la velocidad según el estado
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Movimiento relativo a la cámara
        Vector3 horizVel = new Vector3(moveInput.x, 0, moveInput.y) * currentSpeed;

        if (!Mathf.Approximately(horizVel.magnitude, 0.0f))
        {
            Quaternion rot = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            horizVel = rot * horizVel;
            Quaternion direction = Quaternion.LookRotation(horizVel);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
        }

        controller.Move(horizVel * Time.deltaTime);


        // Gestión de la resistencia                                              // FALTA ARREGLAR EL BUG DE QUE SI LLEGA 0 LA RESISTENCIA PUEDE SPAMEAR EL SHIFT UNA VEZ PASA EL MINIMO
        if (isRunning && horizVel.magnitude > 0.1f && !isTired && currentStamina > 0)
        {
            // Correr y gastar stamina
            currentStamina -= staminaDrain * Time.deltaTime;
            recoveryTimer = 0f; // reiniciar delay de recuperación

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isTired = true;
                isRunning = false; // forzar que deje de correr
                Debug.Log("Me he cansado");
            }
        }
        else
        {
            // Si no puede correr, asegurarse de que camine
            if (currentStamina < minStaminaToRun || isTired)
            {
                isRunning = false;
            }

            // No está corriendo, empieza a contar el delay
            if (recoveryTimer < recoveryDelay)
            {
                recoveryTimer += Time.deltaTime;
            }
            else // ya pasó el delay, puede recuperar stamina
            {
                if (horizVel.magnitude < 0.1f) // quieto
                {
                    currentStamina += staminaRecoveryStill * Time.deltaTime;
                    Debug.Log("Recupero rápido");
                }
                else // caminando
                {
                    currentStamina += staminaRecovery * Time.deltaTime;
                    Debug.Log("Recupero lento");
                }

                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    isTired = false;
                    Debug.Log("Tengo la resistencia al máximo");
                }
            }
        }


        // Aplicar gravedad
        controller.Move(Physics.gravity * Time.deltaTime);

        // Debug de resistencia
        Debug.Log("Stamina: " + currentStamina);
    }
}