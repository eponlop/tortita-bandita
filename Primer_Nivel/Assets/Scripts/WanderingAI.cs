using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // ¡Necesario para las listas de puntos!

[RequireComponent(typeof(CharacterController))]
public class WanderingAI : MonoBehaviour
{
    // --- NUEVAS ESTRUCTURAS DE DATOS ---
    // Usadas por la lógica de trazado de visión
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
    }

    // --- VARIABLES ORIGINALES ---
    [Header("Movimiento")]
    public float speed = 3.0f;
    public float rotationSpeed = 5.0f;
    public float reachDistance = 0.5f;
    public float chaseSpeed = 6.0f;

    private float currentMovementSpeed;

    [Header("Efectos de Estado")]
    private float slowFactor = 1f;
    private bool isSlowed = false;

    [Header("Detección")]
    public float viewAngle = 180f;
    public float viewDistance = 60f;
    public float eyeHeight = 1.5f;
    public LayerMask obstacleMask; // Máscara de capa para las paredes

    [Header("Waypoints")]
    public bool circularPatrol = false;
    public Transform[] waypoints;

    [Header("Audio")]
    public AudioSource footstepsAudioSource;
    public AudioSource stickyAudioSource;
    public float targetPitch = 0.5f;

    private GameObject player;
    private int currentWaypoint = 0;
    private int dirSign = 1;
    private CharacterController controller;

    private bool chasing = false;
    private bool patrolling = false;

    private Animator animator;

    [Header("Referencias del Sistema")]
    public MenuPillado gameManager; // ¡Arrastra el objeto GameManager aquí!

    [Header("Visualización Runtime")]
    public LineRenderer fovRenderer;
    [Range(1, 100)]
    public int raysPerDegree = 1; // Precisión del contorno
    public int segments = 10;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();

        if (fovRenderer == null)
        {
            fovRenderer = GetComponent<LineRenderer>();
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<MenuPillado>();
            if (gameManager == null)
            {
                Debug.LogError("WanderingAI: No se encontró el MenuPillado (GameManager) en la escena.");
            }
        }

        chasing = false;
        patrolling = true;
        isSlowed = false;
        slowFactor = 1f;

        // 1. Configuración Inicial del Audio:
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.pitch = 1.5f;
            footstepsAudioSource.loop = true;
            footstepsAudioSource.Stop();
        }
        if (stickyAudioSource != null)
        {
            stickyAudioSource.loop = true;
            stickyAudioSource.Stop();
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isPatrolling", true);
            animator.Play("patrolling", 0, 0f);
        }
    }

    void Update()
    {
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

        if (animator != null)
        {
            if (chasing)
            {
                ChasePlayer();

                //cambia el color del line render a rojo
                // COMPLETAR AQUI

                
                if (chasing)
                {
                    fovRenderer.material.color = Color.red;
                }
            }
            else
            {
                Patrol();
            }
            animator.SetBool("isChasing", chasing);
            animator.SetBool("isPatrolling", patrolling);
        }
        else
        {
            if (chasing)
                ChasePlayer();
            else
                Patrol();
        }

        // Controlamos qué audioSource debe estar sonando y si debe estar en pausa.
        ManageFootstepAudio();

        GenerateViewPoints();
    }

    public void ApplySlow(float factor)
    {
        if (isSlowed) return;

        slowFactor = factor;
        isSlowed = true;

        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.Stop();
        }
        if (stickyAudioSource != null)
        {
            stickyAudioSource.pitch = targetPitch;
        }
    }

    public void RemoveSlow()
    {
        if (!isSlowed) return;

        slowFactor = 1f;
        isSlowed = false;

        if (stickyAudioSource != null)
        {
            stickyAudioSource.Stop();
        }
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.pitch = 1.5f;
        }
    }

    private void ManageFootstepAudio()
    {
        bool isMoving = currentMovementSpeed * slowFactor > 0.01f;
        AudioSource sourceToPlay = isSlowed ? stickyAudioSource : footstepsAudioSource;
        AudioSource sourceToStop = isSlowed ? footstepsAudioSource : stickyAudioSource;

        if (sourceToStop != null && sourceToStop.isPlaying)
        {
            sourceToStop.Stop();
        }

        if (sourceToPlay != null)
        {
            if (isMoving)
            {
                if (!sourceToPlay.isPlaying)
                {
                    sourceToPlay.UnPause();
                    if (!sourceToPlay.isPlaying) sourceToPlay.Play();
                }
            }
            else
            {
                if (sourceToPlay.isPlaying)
                {
                    sourceToPlay.Pause();
                }
            }
        }
    }

    bool CheckForPlayerFOV()
    {
        if (player == null) return false;

        Vector3 enemyEyePos = transform.position + Vector3.up * eyeHeight;

        // Definimos los puntos de destino en el jugador para la detección.
        // Asumimos que el punto de pivote del jugador está en la parte inferior.
        // 0.2f es cerca de los pies, y 1.7f es cerca de la cabeza (ajusta estos valores)
        Vector3[] targetPoints = new Vector3[]
        {
            player.transform.position + Vector3.up * 0.2f, // Pies/cuerpo bajo
            player.transform.position + Vector3.up * 1.0f, // Centro del cuerpo
            player.transform.position + Vector3.up * 4f  // Cabeza
        };

        foreach (Vector3 targetPos in targetPoints)
        {
            Vector3 directionToTarget = targetPos - enemyEyePos;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget > viewDistance)
            {
                // El punto de destino está fuera de la distancia de visión, saltar al siguiente punto.
                continue;
            }

            Vector3 directionNormalized = directionToTarget.normalized;

            // 1. Comprobación del ángulo Horizontal (Cualquier ángulo fuera del cono se ignora)
            // Para la detección FOV, solo necesitamos comprobar el ángulo en el plano XZ (horizontal).
            // Creamos una proyección plana de la dirección al objetivo y del forward del enemigo.
            Vector3 enemyForwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 dirToTargetFlat = Vector3.ProjectOnPlane(directionToTarget, Vector3.up).normalized;

            float dotProductFlat = Vector3.Dot(enemyForwardFlat, dirToTargetFlat);
            float cosineThreshold = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

            // Si el jugador está fuera del cono horizontal, no lo detectamos.
            if (dotProductFlat < cosineThreshold)
            {
                continue; // El jugador está fuera del ángulo horizontal
            }

            // 2. Comprobación de Oclusión (Raycast)
            RaycastHit hit;
            if (Physics.Raycast(enemyEyePos, directionNormalized, out hit, distanceToTarget, obstacleMask))
            {
                // Si el rayo golpea algo, comprobamos si ese algo es el jugador.
                // Usamos hit.collider.gameObject.CompareTag("Player") para una comprobación más robusta.
                if (hit.collider.gameObject == player)
                {
                    // ¡Detección exitosa!
                    return true;
                }
            }
        }

        // Si después de verificar todos los puntos, ninguno detecta al jugador:
        return false;
    }

    // -------------------------------------------------------------------
    // --- LÓGICA DE DIBUJO AVANZADA (CORREGIDA LA ORIENTACIÓN Y EL CIERRE) ---
    // -------------------------------------------------------------------

    void GenerateViewPoints()
    {
        if (fovRenderer == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        List<Vector3> viewPoints = new List<Vector3>();

        float totalRays = viewAngle * raysPerDegree;
        float stepAngleSize = viewAngle / totalRays;
        // El ángulo de inicio ahora es relativo al forward del enemigo (0 grados es transform.forward)
        float startAngle = -viewAngle / 2;

        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= totalRays; i++)
        {
            float currentAngle = startAngle + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(currentAngle);

            bool hitEdge = oldViewCast.hit != newViewCast.hit;

            if (i > 0 && hitEdge)
            {
                EdgeInfo edge = FindEdge(oldViewCast, newViewCast);

                if (oldViewCast.hit)
                {
                    viewPoints.Add(edge.pointA);
                }
                else
                {
                    viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // 1. Insertamos el origen al inicio. El Line Renderer irá del origen al primer punto.
        viewPoints.Insert(0, origin);

        // 2. CORRECCIÓN DEL CIERRE: La última línea debe ir del último punto visible
        //    de vuelta al origen (viewPoints[0]). Como el Line Renderer dibuja
        //    los puntos en orden, si insertamos el origen al inicio, la última línea 
        //    debe ser del último punto al origen. Para lograr esto, añadimos el origen
        //    al final de la lista.
        viewPoints.Add(origin);

        fovRenderer.positionCount = viewPoints.Count;
        fovRenderer.SetPositions(viewPoints.ToArray());

        if (fovRenderer.material != null)
        {
            fovRenderer.material.color = chasing ? Color.red : Color.yellow;
        }
    }

    // Función auxiliar: Lanza un Raycast y retorna la información
    ViewCastInfo ViewCast(float angleInDegrees) // Recibe el ángulo relativo
    {
        Vector3 dir = DirFromAngle(angleInDegrees); // Usa la función de rotación (Quaternion)
        Vector3 rayStart = transform.position + Vector3.up * eyeHeight;
        RaycastHit hit;

        if (Physics.Raycast(rayStart, dir, out hit, viewDistance, obstacleMask))
        {
            return new ViewCastInfo
            {
                hit = true,
                point = hit.point,
                dst = hit.distance,
                angle = angleInDegrees // Usamos el ángulo relativo
            };
        }
        else
        {
            return new ViewCastInfo
            {
                hit = false,
                point = rayStart + dir * viewDistance,
                dst = viewDistance,
                angle = angleInDegrees // Usamos el ángulo relativo
            };
        }
    }

    // Función auxiliar: Encuentra el punto exacto del borde
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = minViewCast.point;
        Vector3 maxPoint = maxViewCast.point;

        for (int i = 0; i < 10; i++) // Búsqueda binaria para precisión de borde
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (newViewCast.hit == minViewCast.hit)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo { pointA = minPoint, pointB = maxPoint };
    }

    // Función auxiliar: Calcula la dirección del rayo utilizando Quaternion (CORRECCIÓN DE ORIENTACIÓN)
    Vector3 DirFromAngle(float angleInDegrees)
    {
        // 1. Crea una rotación relativa alrededor del eje Y del enemigo (transform.up).
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, transform.up);

        // 2. Multiplica la dirección 'hacia adelante' del enemigo por esta rotación.
        return rotation * transform.forward;
    }

    // -------------------------------------------------------------------
    // --- FUNCIONES ORIGINALES RESTANTES ---
    // -------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (transform == null)
            return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = chasing ? Color.red : Color.yellow;

        float currentViewDistance = viewDistance;
        float currentViewAngle = viewAngle;

        Gizmos.DrawRay(origin, transform.forward * currentViewDistance);

        float halfAngle = currentViewAngle * 0.5f;

        Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, transform.up);
        Vector3 rightDirection = rightRotation * transform.forward;
        Gizmos.DrawRay(origin, rightDirection * currentViewDistance);

        Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, transform.up);
        Vector3 leftDirection = leftRotation * transform.forward;
        Gizmos.DrawRay(origin, leftDirection * currentViewDistance);

        Vector3 rightPoint = origin + rightDirection * currentViewDistance;
        Vector3 leftPoint = origin + leftDirection * currentViewDistance;
        Gizmos.DrawLine(rightPoint, leftPoint);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(origin, 0.1f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject == player)
        {
            Debug.Log("¡El enemigo atrapó al jugador!");

            if (gameManager != null)
            {
                // **CAMBIO CRÍTICO:** Llama al GameManager para manejar el fin del juego.
                gameManager.OnEnemyCaughtPlayer();
            }
            else
            {
                // Si el GameManager es null, haz la recarga de emergencia.
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            Debug.Log("¡El jugador entró en el rango del enemigo!");
        }
    }

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

            if (circularPatrol)
            {
                currentWaypoint = currentWaypoint % waypoints.Length;
            } 
            else if (currentWaypoint >= waypoints.Length)
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

        currentMovementSpeed = speed;
        Vector3 move = transform.forward * currentMovementSpeed * slowFactor * Time.deltaTime;
        controller.Move(move);
    }

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

            currentMovementSpeed = chaseSpeed;
            Vector3 move = transform.forward * currentMovementSpeed * slowFactor * Time.deltaTime;
            controller.Move(move);
        }
        else
        {
            currentMovementSpeed = 0f;
        }
    }
}