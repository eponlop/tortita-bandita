using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para la línea de SceneManager.LoadScene

[RequireComponent(typeof(CharacterController))]
public class WanderingAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 3.0f;
    public float rotationSpeed = 5.0f;
    public float reachDistance = 0.5f;
    public float chaseSpeed = 6.0f;

    [Header("Detección")]
    public float viewAngle = 180f; // Ángulo total del cono de visión
    public float viewDistance = 60f; // Distancia máxima de visión
    public float eyeHeight = 1.5f; // Altura del rayo de visión

    [Header("Waypoints")]
    public Transform[] waypoints;

    private GameObject player;
    private int currentWaypoint = 0;
    private int dirSign = 1; // +1 = adelante, -1 = atrás
    private CharacterController controller;

    private bool chasing = false;
    private bool patrolling = false;

    private Animator animator;

    [Header("Visualización Runtime")] // Necesario para LineRenderer
    public LineRenderer fovRenderer;
    public int segments = 10; // Para suavizar el arco del cono


    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 1. Obtener la referencia al jugador y al Animator
        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();

        // 2. Asegurar que el LineRenderer esté asignado
        if (fovRenderer == null)
        {
            fovRenderer = GetComponent<LineRenderer>();
        }

        // ------------------ REINICIO DE ESTADO CLAVE (SOLUCIÓN) ------------------
        // Esto asegura que, después de cargar la escena, la IA empiece
        // siempre en estado de patrulla (no persiguiendo).
        chasing = false;
        patrolling = true;

        // 3. Reiniciar el estado de las animaciones
        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isPatrolling", true);
            // Opcional: poner el controlador de vuelta al frame inicial
            animator.Play("Idle", 0, 0f);
        }
    }

    void Update()
    {
        // ------------------ LÓGICA DE DETECCIÓN CON CONO ------------------
        bool playerDetected = CheckForPlayerFOV();

        if (playerDetected)
        {
            chasing = true;
            patrolling = false;
        }
        else
        {
            chasing = false;
            patrolling = true;
        }

        // ------------------ EJECUCIÓN DEL COMPORTAMIENTO ------------------
        if (chasing)
        {
            ChasePlayer();
            animator.SetBool("isChasing", chasing);
            animator.SetBool("isPatrolling", patrolling);
        }
        else
        {
            Patrol();
            animator.SetBool("isChasing", chasing);
            animator.SetBool("isPatrolling", patrolling);
        }

        // Llamada para dibujar el FOV en cada frame (si LineRenderer existe)
        DrawFOVRuntime();
    }

    // ---------------- FUNCIÓN: CONO DE VISIÓN (FOV) ----------------
    bool CheckForPlayerFOV()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 1. Control de la Distancia
        if (distanceToPlayer > viewDistance)
        {
            return false;
        }

        // 2. Control del Ángulo (Cono de Visión)
        Vector3 directionNormalized = directionToPlayer.normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionNormalized);
        float cosineThreshold = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        if (dotProduct > cosineThreshold)
        {
            // 3. Verificación de Obstrucciones (Raycast desde la altura de los ojos)
            Vector3 rayStartPoint = transform.position + Vector3.up * eyeHeight;

            RaycastHit hit;
            if (Physics.Raycast(rayStartPoint, directionNormalized, out hit, distanceToPlayer))
            {
                if (hit.collider.gameObject == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ---------------- DIBUJO DEL CAMPO DE VISIÓN EN JUEGO (RUNTIME) ----------------
    void DrawFOVRuntime()
    {
        if (fovRenderer == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        int pointCount = segments + 3; // Origen + Arco + Cierre

        fovRenderer.positionCount = pointCount;
        Vector3[] points = new Vector3[pointCount];
        points[0] = origin;

        float angleStep = viewAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -viewAngle / 2 + angleStep * i;

            // Rotar el vector forward (transform.forward) por el ángulo alrededor del eje Y (transform.up)
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, transform.up);
            Vector3 direction = rotation * transform.forward;

            Vector3 pointOnArc = origin + direction * viewDistance;

            points[i + 1] = pointOnArc;
        }

        // Cierra el cono volviendo al origen
        points[segments + 2] = origin;

        fovRenderer.SetPositions(points);

        // Opcional: Cambiar el color según el estado
        if (fovRenderer.material != null)
        {
            fovRenderer.material.color = chasing ? Color.red : Color.yellow;
        }
    }


    // ---------------- DEPURACIÓN: DIBUJAR CAMPO DE VISIÓN (GIZMOS - Solo Editor) ----------------
    private void OnDrawGizmos()
    {
        if (transform == null)
            return;

        // 1. Definir el punto de inicio del rayo (altura de los ojos)
        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        // 2. Dibujar el rango de visión (color rojo si persigue, amarillo si patrulla)
        Gizmos.color = chasing ? Color.red : Color.yellow;

        float currentViewDistance = viewDistance;
        float currentViewAngle = viewAngle;

        // 3. Dibujar la línea central de visión
        Gizmos.DrawRay(origin, transform.forward * currentViewDistance);

        // 4. Dibujar los límites del cono
        float halfAngle = currentViewAngle * 0.5f;

        // Límite derecho
        Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, transform.up);
        Vector3 rightDirection = rightRotation * transform.forward;
        Gizmos.DrawRay(origin, rightDirection * currentViewDistance);

        // Límite izquierdo
        Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, transform.up);
        Vector3 leftDirection = leftRotation * transform.forward;
        Gizmos.DrawRay(origin, leftDirection * currentViewDistance);

        // 5. Dibujar un arco de conexión (la "tapa" del cono)
        Vector3 rightPoint = origin + rightDirection * currentViewDistance;
        Vector3 leftPoint = origin + leftDirection * currentViewDistance;
        Gizmos.DrawLine(rightPoint, leftPoint);

        // Opcional: Dibujar una esfera en el punto de origen
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(origin, 0.1f);
    }

    // ---------------- COLISIONES ----------------
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject == player)
        {
            Debug.Log("¡El enemigo atrapó al jugador!");
            // Asumo que esta línea es para reiniciar la escena al atrapar al jugador
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            Debug.Log("¡El jugador entró en el rango del enemigo!");
        }
    }

    // ---------------- PATRULLA ENTRE WAYPOINTS ----------------
    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Vector3 targetPos = waypoints[currentWaypoint].position;
        Vector3 dirToTarget = targetPos - transform.position;
        dirToTarget.y = 0f;

        float distance = dirToTarget.magnitude;

        if (distance < reachDistance)
        {
            currentWaypoint += dirSign;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = waypoints.Length - 2;
                dirSign = -1;
            }
            else if (currentWaypoint < 0)
            {
                currentWaypoint = 1;
                dirSign = 1;
            }
        }

        if (dirToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime * 100f
            );
        }

        Vector3 move = transform.forward * speed * Time.deltaTime;
        controller.Move(move);
    }

    // ---------------- PERSECUCIÓN ----------------
    void ChasePlayer()
    {
        if (player == null) return;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > 1.0f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5.0f
            );

            Vector3 move = transform.forward * chaseSpeed * Time.deltaTime;
            controller.Move(move);
        }
    }
}