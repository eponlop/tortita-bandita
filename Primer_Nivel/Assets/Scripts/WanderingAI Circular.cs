using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // ¡Necesario para las listas de puntos!

[RequireComponent(typeof(CharacterController))]
public class WanderingAICircular : MonoBehaviour
{
    // --- NUEVAS ESTRUCTURAS DE DATOS ---
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

    // --- CONFIGURACIÓN DEL ENEMIGO ---
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
    public Transform[] waypoints;

    [Header("Audio")]
    public AudioSource footstepsAudioSource;
    public AudioSource stickyAudioSource;
    public float targetPitch = 0.5f;

    // --- REFERENCIAS DE COMPONENTES Y ESTADO ---
    private GameObject player;
    private int currentWaypoint = 0;
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

    // ====================================================================
    // START & UPDATE
    // ====================================================================

    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();

        if (fovRenderer == null)
            fovRenderer = GetComponent<LineRenderer>();

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

        // Configuración Inicial del Audio:
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

        chasing = playerDetected;
        patrolling = !playerDetected;

        if (animator != null)
        {
            if (chasing)
            {
                ChasePlayer();
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

        // Controlamos el audio y la visualización
        ManageFootstepAudio();
        GenerateViewPoints();
    }

    // ====================================================================
    // LÓGICA DE MOVIMIENTO Y ESTADO
    // ====================================================================

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
            // --- LÓGICA DE PATRULLA CIRCULAR (Loop) ---
            currentWaypoint++;
            // Vuelve a 0 si alcanza el final del array
            currentWaypoint = currentWaypoint % waypoints.Length;
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

    // --- (Funciones de Lógica de Estado: ApplySlow, RemoveSlow, ManageFootstepAudio) ---
    public void ApplySlow(float factor)
    {
        if (isSlowed) return;
        slowFactor = factor;
        isSlowed = true;
        if (footstepsAudioSource != null) footstepsAudioSource.Stop();
        if (stickyAudioSource != null) stickyAudioSource.pitch = targetPitch;
    }

    public void RemoveSlow()
    {
        if (!isSlowed) return;
        slowFactor = 1f;
        isSlowed = false;
        if (stickyAudioSource != null) stickyAudioSource.Stop();
        if (footstepsAudioSource != null) footstepsAudioSource.pitch = 1.5f;
    }

    private void ManageFootstepAudio()
    {
        bool isMoving = currentMovementSpeed * slowFactor > 0.01f;
        AudioSource sourceToPlay = isSlowed ? stickyAudioSource : footstepsAudioSource;
        AudioSource sourceToStop = isSlowed ? footstepsAudioSource : stickyAudioSource;

        if (sourceToStop != null && sourceToStop.isPlaying) sourceToStop.Stop();

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
                if (sourceToPlay.isPlaying) sourceToPlay.Pause();
            }
        }
    }


    // ====================================================================
    // DETECCIÓN (FOV)
    // ====================================================================

    bool CheckForPlayerFOV()
    {
        if (player == null) return false;

        Vector3 enemyEyePos = transform.position + Vector3.up * eyeHeight;

        // Puntos de destino en el jugador para detección 3D (Cabeza, Centro, Pies)
        Vector3[] targetPoints = new Vector3[]
        {
            player.transform.position + Vector3.up * 0.2f,
            player.transform.position + Vector3.up * 1.0f,
            player.transform.position + Vector3.up * 4f
        };

        foreach (Vector3 targetPos in targetPoints)
        {
            Vector3 directionToTarget = targetPos - enemyEyePos;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget > viewDistance) continue;

            Vector3 directionNormalized = directionToTarget.normalized;

            // 1. Comprobación de Ángulo Horizontal (FOV 2D)
            Vector3 enemyForwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 dirToTargetFlat = Vector3.ProjectOnPlane(directionToTarget, Vector3.up).normalized;

            float dotProductFlat = Vector3.Dot(enemyForwardFlat, dirToTargetFlat);
            float cosineThreshold = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

            if (dotProductFlat < cosineThreshold) continue;

            // 2. Comprobación de Oclusión (Raycast 3D)
            RaycastHit hit;
            if (Physics.Raycast(enemyEyePos, directionNormalized, out hit, distanceToTarget, obstacleMask))
            {
                if (hit.collider.gameObject == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ====================================================================
    // VISUALIZACIÓN (Line Renderer)
    // ====================================================================

    void GenerateViewPoints()
    {
        if (fovRenderer == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        List<Vector3> viewPoints = new List<Vector3>();

        float totalRays = viewAngle * raysPerDegree;
        float stepAngleSize = viewAngle / totalRays;
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
                viewPoints.Add(oldViewCast.hit ? edge.pointA : edge.pointB);
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // Cierre del cono: añadir el origen al inicio y al final
        viewPoints.Insert(0, origin);
        viewPoints.Add(origin);

        fovRenderer.positionCount = viewPoints.Count;
        fovRenderer.SetPositions(viewPoints.ToArray());

        // --- SOLUCIÓN ROBUSTA DE COLOR ---
        Color newColor = chasing ? Color.red : Color.yellow;

        // Usamos StartColor/EndColor, que es más fiable en el build que .material.color
        fovRenderer.startColor = newColor;
        fovRenderer.endColor = newColor;
    }

    ViewCastInfo ViewCast(float angleInDegrees)
    {
        Vector3 dir = DirFromAngle(angleInDegrees);
        Vector3 rayStart = transform.position + Vector3.up * eyeHeight;
        RaycastHit hit;

        if (Physics.Raycast(rayStart, dir, out hit, viewDistance, obstacleMask))
        {
            return new ViewCastInfo
            {
                hit = true,
                point = hit.point,
                dst = hit.distance,
                angle = angleInDegrees
            };
        }
        else
        {
            return new ViewCastInfo
            {
                hit = false,
                point = rayStart + dir * viewDistance,
                dst = viewDistance,
                angle = angleInDegrees
            };
        }
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = minViewCast.point;
        Vector3 maxPoint = maxViewCast.point;

        for (int i = 0; i < 10; i++) // Búsqueda binaria
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

    Vector3 DirFromAngle(float angleInDegrees)
    {
        // Rota el vector 'hacia adelante' del enemigo por el ángulo relativo.
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, transform.up);
        return rotation * transform.forward;
    }

    // ====================================================================
    // COLISIONES
    // ====================================================================

    private void OnDrawGizmos()
    {
        // Esta función dibuja el FOV en el editor (sin cambios)
        if (transform == null) return;
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = chasing ? Color.red : Color.yellow;
        // ... (resto de la lógica de Gizmos)
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject == player)
        {
            Debug.Log("¡El enemigo atrapó al jugador!");

            if (gameManager != null)
            {
                // Llama al GameManager para mostrar el menú de capturado y detener el juego.
                gameManager.OnEnemyCaughtPlayer();
            }
            else
            {
                // Alternativa de emergencia si no hay GameManager
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
}