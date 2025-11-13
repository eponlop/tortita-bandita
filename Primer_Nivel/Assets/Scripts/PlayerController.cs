using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private float currentSpeed;
    private bool isRunning;
    private bool isDeformed;

    public float walkSpeed = 5f;
    public float runSpeed = 10f;


    public float currentStamina;                // Resistencia actual
    public float recoverRate;                   // Tasa de recuperación actual

    public float maxStamina = 100f;             // Resistencia máxima
    public float minStaminaToRun = 25f;         // Resistencia mínima para poder correr
    public float staminaDrain = 20f;            // Por segundo al correr    
    public float recoveryDelay = 2f;            // Segundos antes de empezar a recuperar
    public float recoveryTimer = 0f;            // Temporizador de recuperación
    public float staminaRecovery = 7f;          // Por segundo al no correr
    public float staminaRecoveryStill = 15f;    // Por segundo al estar quieto
    public bool isTired = false;

    private Vector3 originalScale;             // Escala original del jugador
    public Vector3 targetScale = new Vector3(2f, 2f, 2f);


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
        if (context.performed && currentStamina >= minStaminaToRun && !isTired)
        {
            isRunning = true;
            //Debug.Log("Running");
        }

        else if (context.canceled)
        {
            isRunning = false;
            //Debug.Log("BackToWalk");
        }
    }

    public void OnDeformarse(InputAction.CallbackContext context)
    {
        // Aquí iría la lógica para deformarse
       if (context.performed)
       {
            isDeformed = true;
            Debug.Log("Deformado");
        }
        else if (context.canceled)
        {
            isDeformed = false;
            Debug.Log("No deformado");
        }
    }

    private void Update()
    {
        // Selecciona la velocidad según el estado
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Movimiento relativo a la cámara
        Vector3 horizVel = new Vector3(moveInput.x, 0, moveInput.y) * currentSpeed;

        bool isMoving = moveInput.sqrMagnitude > 0.1f; // Umbral para considerar movimiento

        if (isMoving)
        {
            Quaternion rot = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            horizVel = rot * horizVel;
            Quaternion direction = Quaternion.LookRotation(horizVel);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
        }

        controller.Move(horizVel * Time.deltaTime);


        // Gestión de la resistencia
        if (isRunning && isMoving && !isTired && currentStamina > 0f)
        {
            // Correr y gastar stamina
            currentStamina -= staminaDrain * Time.deltaTime;
            recoveryTimer = 0f; // reiniciar delay de recuperación

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isTired = true;
                isRunning = false; // forzar que deje de correr
                //Debug.Log("Me he cansado");
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
                recoverRate = isMoving ? staminaRecovery : staminaRecoveryStill;
                currentStamina += recoverRate * Time.deltaTime;

            }
            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isTired = false;
                //Debug.Log("Tengo la resistencia al máximo");
              
            }
        }

        if (isDeformed)
        {
            // Lógica de deformación 
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 5f);
        }
        else {
            if (originalScale == Vector3.zero)
                originalScale = transform.localScale; // Guardar la escala al inicio
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);
        }
        
        
        
        // Aplicar gravedad
            controller.Move(Physics.gravity * Time.deltaTime);

        // Debug de resistencia
        Debug.Log($"Stamina: {currentStamina:F1} | Moving: {isMoving} | Running: {isRunning} | Tired: {isTired} | Timer: {recoveryTimer:F2} | recoverRate: {recoverRate:F1}");
    }
}