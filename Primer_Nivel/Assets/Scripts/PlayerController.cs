using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Animación y Velocidad
    private Animator animator;
    private float currentSpeed;
    private bool isRunning;
    private bool isTurned;
    private bool isCrawling;

    [Header("Velocidad")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;


    private float originalHeight;
    private float originalCenterY;

    float crouchHeight = 0.12f;
    float crouchCenterY = 0.05f;


    [Header("Stamina")]
    public float currentStamina;         // Resistencia actual
    public float recoverRate;            // Tasa de recuperación actual
    public float maxStamina = 100f;      // Resistencia máxima
    public float minStaminaToRun = 25f;  // Resistencia mínima para poder correr/deformarse
    public float staminaDrain = 20f;     // Por segundo al correr
    public float turnedStaminaDrain = 10f; // Por segundo al estar deformado
    public float recoveryDelay = 2f;     // Segundos antes de empezar a recuperar
    public float recoveryTimer = 0f;     // Temporizador de recuperación
    public float staminaRecovery = 7f;   // Por segundo al no correr
    public float staminaRecoveryStill = 15f; // Por segundo al estar quieto
    public bool isTired = false;

    [Header("Character Controller")]
    public float crawlingSpeedMultiplier = 0.5f; // Multiplicador de velocidad al arrastrarse

    // Character Controller Originales
    private float originalRadius;

    // Variables de Posición y Controller (mantener si son usadas en otros lugares)
    private CharacterController controller;
    [SerializeField] private Transform camera;
    public float rotSpeed = 15.0f;
    private Vector2 moveInput;

    // Componente externo (Barra de UI)
    public BarraStamina barra;

    // Variables de Escala y Altura (eliminadas para simplificar, no afectan la lógica de Stamina/Radio)

    /*
    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        currentStamina = maxStamina;

        originalHeight = controller.height;
        originalCenterY = controller.center.y;

        animator = GetComponent<Animator>();

        originalRadius = controller.radius;

        barra.SetMaxStamina(maxStamina);
        barra.SetStamina(currentStamina);

        Debug.Log("Animator controller: " + animator.runtimeAnimatorController);
    }*/

    private IEnumerator Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        currentStamina = maxStamina;

        originalHeight = controller.height;
        originalCenterY = controller.center.y;

        animator = GetComponent<Animator>();

        originalRadius = controller.radius;

        barra.SetMaxStamina(maxStamina);
        barra.SetStamina(currentStamina);

        Debug.Log("Animator controller: " + animator.runtimeAnimatorController);

        // IMPORTANTE — Esperar 1 frame
        yield return null;

        // Reparación del Animator tras recargar escena
        animator.Rebind();
        animator.Update(0f);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
        animator.speed = 1f;

        // Forzar animación inicial
        animator.Play("idle", 0, 0f);

        Debug.Log("Animator reparado después de recargar.");
    }


    // --- INPUTS ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        // Bloqueo de inicio: Cansado, baja stamina, o arrastrándose.
        if (context.performed && currentStamina >= minStaminaToRun && !isTired && !isCrawling)
        {
            isRunning = true;
            animator.SetBool("IsRunning", isRunning);

            if (isCrawling)
            {
                isCrawling = false;
                animator.SetBool("IsCrawling", isCrawling);
            }
        }
        else if (context.canceled)
        {
            isRunning = false;
            animator.SetBool("IsRunning", isRunning);
        }
    }

    public void OnDeformarse(InputAction.CallbackContext context)
    {
        // Solo establece el parámetro del Animator si las condiciones son válidas.
        if (context.performed)
        {
            // Verificación de inicio
            if (!isTired && currentStamina >= minStaminaToRun)
            {
                animator.SetBool("IsDeformed", true);

                if (isRunning)
                {
                    isRunning = false;
                    animator.SetBool("IsRunning", isRunning);
                }

                // Sincronizar la variable del script (para uso en Update y otros scripts)
                isTurned = true;
            }
            else
            {
                // Si el jugador pulsa y no puede (cansado/baja stamina)
                animator.SetBool("IsDeformed", false);
                isTurned = false;
            }
        }
        else if (context.canceled)
        {
            // Cancelar siempre quita el parámetro
            animator.SetBool("IsDeformed", false);
            isTurned = false;
        }
    }

    public void OnArrastrarse(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            if (!isRunning)
            {
                isCrawling = true;
                animator.SetBool("IsCrawling", isCrawling);

                controller.height = crouchHeight;
                controller.center = new Vector3(0, crouchCenterY, 0);
            }
        }
        else if (context.canceled)
        {
            controller.height = originalHeight;
            controller.center = new Vector3(0, originalCenterY, 0);
            isCrawling = false;
            animator.SetBool("IsCrawling", isCrawling);
        }
    }

    // --- UPDATE ---

    private void Update()
    {
        // -----------------------------------------------------------------
        // 1. GESTIÓN DE ESTADOS Y BLOQUEO DE INICIO
        // -----------------------------------------------------------------

        // Bloqueo Forzado: Si isTired está activo, forzamos la desactivación de Correr y Deformarse.
        if (isTired)
        {
            isRunning = false;
            isTurned = false;
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsDeformed", false); // Forzamos el Animator a salir
        }

        // Lectura del estado real del Animator: Usamos este valor para aplicar efectos y drenaje.
        bool isCurrentlyDeformed = animator.GetBool("IsDeformed");

        // -----------------------------------------------------------------
        // 2. CÁLCULO DE VELOCIDAD Y RADIO 
        // -----------------------------------------------------------------

        // Selecciona la velocidad base: Correr > Caminar
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Si el Animator está actualmente deformado (la transición fue válida)
        if (isCurrentlyDeformed)
        {
            controller.radius = 0.025f;
            currentSpeed = walkSpeed / 1.3f;

            // Deformarse tiene prioridad: si corre, lo cancela.
            if (isRunning)
            {
                isRunning = false;
                animator.SetBool("IsRunning", isRunning);
            }
        }
        else
        {
            // Volver al radio original
            controller.radius = originalRadius;
        }

        // -----------------------------------------------------------------
        // 3. MOVIMIENTO Y ROTACIÓN
        // -----------------------------------------------------------------

        Vector3 horizVel = new Vector3(moveInput.x, 0, moveInput.y) * currentSpeed;
        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        animator.SetFloat("Speed", horizVel.sqrMagnitude);

        if (isMoving && isCrawling)
        {
            Quaternion rot = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            horizVel = rot * horizVel * crawlingSpeedMultiplier;
            Quaternion direction = Quaternion.LookRotation(horizVel);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
        }
        else if (isMoving)
        {
            Quaternion rot = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            horizVel = rot * horizVel;
            Quaternion direction = Quaternion.LookRotation(horizVel);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
        }

        controller.Move(horizVel * Time.deltaTime);

        // -----------------------------------------------------------------
        // 4. GESTIÓN DE LA RESISTENCIA (Drenaje y Recuperación)
        // -----------------------------------------------------------------

        bool isStaminaDraining = false;
        float drainRate = 0f;

        // A. Drenaje por Correr (Solo si no está cansado)
        if (isRunning && isMoving && !isTired && !isCrawling && currentStamina > 0f)
        {
            isStaminaDraining = true;
            drainRate = staminaDrain;
        }
        // B. Drenaje por Deformarse (Solo si no está cansado)
        else if (isCurrentlyDeformed && !isTired && currentStamina > 0f)
        {
            isStaminaDraining = true;
            drainRate = turnedStaminaDrain;
        }

        if (isStaminaDraining)
        {
            // Gasto de Stamina
            currentStamina -= drainRate * Time.deltaTime;
            recoveryTimer = 0f; // reiniciar delay de recuperación

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isTired = true; // Cansancio total (Penalización)

                // Forzar desactivación total al agotamiento
                isRunning = false;
                animator.SetBool("IsRunning", isRunning);
                animator.SetBool("IsDeformed", false);

                // ** CORRECCIÓN: Forzar el radio a su valor original inmediatamente **
                controller.radius = originalRadius;

                animator.SetBool("IsTired", isTired);
            }
        }
        else // No está gastando stamina
        {
            // No está corriendo/deformándose, empieza a contar el delay
            if (recoveryTimer < recoveryDelay)
            {
                recoveryTimer += Time.deltaTime;
            }
            else // ya pasó el delay, puede recuperar stamina
            {
                recoverRate = isMoving ? staminaRecovery : staminaRecoveryStill;
                currentStamina += recoverRate * Time.deltaTime;
            }

            // Solo sale de isTired cuando la Stamina es MAXIMA
            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                if (isTired)
                {
                    isTired = false;
                    animator.SetBool("IsTired", isTired);
                }
            }
        }

        if (barra != null)
            barra.SetStamina(currentStamina);

        // -----------------------------------------------------------------
        // 5. APLICAR GRAVEDAD
        // -----------------------------------------------------------------

        controller.Move(Physics.gravity * Time.deltaTime);
    }
}