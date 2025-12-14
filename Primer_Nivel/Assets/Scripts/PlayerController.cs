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

    // ⭐ NUEVAS VARIABLES DE CONTROL DE RALENTIZACIÓN
    private float slowFactor = 1f; // 1.0 = normal, 0.5 = 50% de velocidad
    private bool isSlowed = false;

    [Header("Velocidad")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;


    private float originalHeight;
    private float originalCenterY;

    float crouchHeight = 0.12f;
    float crouchCenterY = 0.05f;


    [Header("Stamina")]
    public float currentStamina;
    public float recoverRate;
    public float maxStamina = 100f;
    public float minStaminaToRun = 25f;
    public float staminaDrain = 20f;
    public float turnedStaminaDrain = 10f;
    public float recoveryDelay = 2f;
    public float recoveryTimer = 0f;
    public float staminaRecovery = 15f;
    public float staminaRecoveryStill = 25f;
    public bool isTired = false;

    [Header("Character Controller")]
    public float crawlingSpeedMultiplier = 0.5f;

    // Character Controller Originales
    private float originalRadius;

    // Variables de Posición y Controller 
    private CharacterController controller;
    [SerializeField] private Transform camera;
    public float rotSpeed = 15.0f;
    private Vector2 moveInput;

    // Componente externo (Barra de UI)
    public BarraStamina barra;

    // =========================================================================
    // 🍯 HABILIDAD: MANCHA DE SIROPE
    // =========================================================================

    [Header("Habilidad de Sirope")]
    [Tooltip("Prefab de la mancha de sirope a instanciar.")]
    public GameObject siropePrefab;

    [Tooltip("Distancia detrás del jugador donde se colocará la mancha.")]
    public float siropeOffsetDistance = 1.0f;

    [Tooltip("Costo de Stamina para colocar una mancha.")]
    public float siropeStaminaCost = 15f;

    // =========================================================================


    private void Start()
    {
        // 1. OBTENCIÓN DE REFERENCIAS
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // 2. REINICIO EXPLÍCITO DE ESTADO DEL SCRIPT
        isRunning = false;
        isTurned = false;
        isCrawling = false;
        isTired = false;

        // ⭐ Estado inicial de ralentización
        isSlowed = false;
        slowFactor = 1f;

        // Reinicio de variables de control de movimiento y stamina
        currentSpeed = walkSpeed;
        currentStamina = maxStamina;
        recoveryTimer = 0f;

        // 3. INICIALIZACIÓN DE VALORES ORIGINALES (CharacterController)
        originalHeight = controller.height;
        originalCenterY = controller.center.y;
        originalRadius = controller.radius;

        // Asegurar que el CharacterController esté en el estado 'caminando'
        controller.height = originalHeight;
        controller.center = new Vector3(0, originalCenterY, 0);
        controller.radius = originalRadius;


        // 4. SINCRONIZACIÓN Y REINICIO DEL ANIMATOR
        if (animator != null)
        {
            animator.Rebind();

            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsTired", isTired);
            animator.SetBool("IsCrawling", isCrawling);
            animator.SetBool("IsDeformed", isTurned);
            animator.SetFloat("Speed", 0f);

            animator.Play("idle", 0, 0f);
        }

        // 5. INICIALIZACIÓN DE LA UI (Barra de Stamina)
        if (barra != null)
        {
            barra.SetMaxStamina(maxStamina);
            barra.SetStamina(currentStamina);
        }
    }


    // --- INPUTS ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDropSirope(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Bloqueamos la acción si el jugador está cansado.
            if (isTired)
            {
                Debug.Log("No se puede soltar sirope, el jugador está agotado.");
                return;
            }

            // 1. Verificación de Coste: ¿Hay suficiente Stamina?
            if (currentStamina >= siropeStaminaCost)
            {
                // 2. Colocar el sirope y gastar Stamina
                DropSiropeBehind();
                currentStamina -= siropeStaminaCost;

                // Reiniciar el contador de recuperación (inicia el delay)
                recoveryTimer = 0f;

                // Actualizar la UI inmediatamente
                if (barra != null)
                    barra.SetStamina(currentStamina);
            }
            else
            {
                Debug.Log("Stamina insuficiente para soltar sirope. Se requieren: " + siropeStaminaCost);
            }
        }
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

        // ⭐ Aplicamos el factor de ralentización AQUI
        float finalSpeed = currentSpeed * slowFactor;

        // -----------------------------------------------------------------
        // 3. MOVIMIENTO Y ROTACIÓN
        // -----------------------------------------------------------------

        // ⭐ Usamos finalSpeed en lugar de currentSpeed
        Vector3 horizVel = new Vector3(moveInput.x, 0, moveInput.y) * finalSpeed;
        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        animator.SetFloat("Speed", horizVel.sqrMagnitude);

        if (isMoving && isCrawling)
        {
            Quaternion rot = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            // ⭐ Aplicamos el factor de arrastrarse (crawlingSpeedMultiplier) al finalSpeed
            horizVel = rot * new Vector3(moveInput.x, 0, moveInput.y) * finalSpeed * crawlingSpeedMultiplier;
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

                // Forzar el radio a su valor original inmediatamente
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

    // =========================================================================
    // ⭐ NUEVA LÓGICA: RALENTIZACIÓN (para interacción con SiropeEffect.cs)
    // =========================================================================

    /// <summary>
    /// Aplica un factor de ralentización al jugador. 
    /// Llamado por el script SiropeEffect cuando el jugador entra en el Trigger.
    /// </summary>
    /// <param name="factor">El multiplicador de velocidad (ej: 0.5 para 50%).</param>
    public void ApplySlow(float factor)
    {
        if (isSlowed) return; // Ya está ralentizado

        slowFactor = factor;
        isSlowed = true;
        Debug.Log("Jugador ralentizado.");
    }

    /// <summary>
    /// Elimina el efecto de ralentización.
    /// Llamado por el script SiropeEffect cuando el jugador sale del Trigger.
    /// </summary>
    public void RemoveSlow()
    {
        if (!isSlowed) return; // No estaba ralentizado

        slowFactor = 1f; // Vuelve a velocidad normal
        isSlowed = false;
        Debug.Log("Ralentización del jugador eliminada.");
    }


    // =========================================================================
    // ⚙️ LÓGICA DE HABILIDAD: SOLTAR SIROPE
    // =========================================================================

    /// <summary>
    /// Calcula la posición detrás del jugador e instancia el prefab del sirope.
    /// </summary>
    void DropSiropeBehind()
    {
        if (siropePrefab == null)
        {
            Debug.LogError("¡ERROR! El Prefab de Sirope no está asignado en el Inspector.");
            return;
        }

        // 1. Calcular la posición: Posición actual - (Dirección hacia atrás * Distancia de separación)
        Vector3 positionBehind = transform.position + (transform.forward * -1f * siropeOffsetDistance);

        // 2. Ajustar la altura 'Y' para que quede en el suelo
        positionBehind.y = transform.position.y - (controller.height / 2f) + 0.05f;

        // 3. Instanciar el Prefab con la rotación del jugador
        Instantiate(siropePrefab, positionBehind, transform.rotation);

        Debug.Log("Mancha de sirope colocada detrás del jugador.");
    }
}