using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class WanderingAI : MonoBehaviour
{
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

    [Header("Waypoints")]
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

    [Header("Visualización Runtime")]
    public LineRenderer fovRenderer;
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

        chasing = false;
        patrolling = true;
        isSlowed = false;
        slowFactor = 1f;

        // 1. Configuración Inicial del Audio:
        // Aseguramos que AMBOS estén configurados para LOOP.
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.pitch = 1.5f;
            footstepsAudioSource.loop = true;
            footstepsAudioSource.Stop(); // Detenemos ambos para que solo ManageFootstepAudio los inicie.
        }
        if (stickyAudioSource != null)
        {
            stickyAudioSource.loop = true;
            stickyAudioSource.Stop();
        }

        // 2. Iniciamos el Audio de Footsteps (solo si el enemigo se mueve al inicio)
        // La reproducción real se maneja en ManageFootstepAudio()

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

        DrawFOVRuntime();
    }

    public void ApplySlow(float factor)
    {
        if (isSlowed) return;

        slowFactor = factor;
        isSlowed = true;

        // CAMBIO DE AUDIO SOURCE: Detiene Footsteps, Inicia Sticky
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.Stop();
        }
        if (stickyAudioSource != null)
        {
            stickyAudioSource.pitch = targetPitch;
            // No usamos .Play() aquí, ya que ManageFootstepAudio lo controlará
        }
    }

    public void RemoveSlow()
    {
        if (!isSlowed) return;

        slowFactor = 1f;
        isSlowed = false;

        // CAMBIO DE AUDIO SOURCE: Detiene Sticky, Inicia Footsteps
        if (stickyAudioSource != null)
        {
            stickyAudioSource.Stop();
        }
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.pitch = 1.5f;
            // No usamos .Play() aquí, ya que ManageFootstepAudio lo controlará
        }
    }

    private void ManageFootstepAudio()
    {
        // La velocidad de movimiento es la velocidad de movimiento real aplicada.
        bool isMoving = currentMovementSpeed * slowFactor > 0.01f;

        // El AudioSource que DEBE estar activo
        AudioSource sourceToPlay = isSlowed ? stickyAudioSource : footstepsAudioSource;

        // El AudioSource que DEBE estar detenido
        AudioSource sourceToStop = isSlowed ? footstepsAudioSource : stickyAudioSource;

        // 1. Detenemos el AudioSource INACTIVO
        if (sourceToStop != null && sourceToStop.isPlaying)
        {
            sourceToStop.Stop();
        }

        // 2. Controlamos la reproducción del AudioSource ACTIVO
        if (sourceToPlay != null)
        {
            if (isMoving)
            {
                // Si se mueve, aseguramos que se esté reproduciendo o se reanude
                if (!sourceToPlay.isPlaying)
                {
                    sourceToPlay.UnPause();
                    if (!sourceToPlay.isPlaying) sourceToPlay.Play();
                }
            }
            else
            {
                // Si está quieto, pausamos el audio
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

        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
        {
            return false;
        }

        Vector3 directionNormalized = directionToPlayer.normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionNormalized);
        float cosineThreshold = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        if (dotProduct > cosineThreshold)
        {
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

    void DrawFOVRuntime()
    {
        if (fovRenderer == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        int pointCount = segments + 3;

        fovRenderer.positionCount = pointCount;
        Vector3[] points = new Vector3[pointCount];
        points[0] = origin;

        float angleStep = viewAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -viewAngle / 2 + angleStep * i;

            Quaternion rotation = Quaternion.AngleAxis(currentAngle, transform.up);
            Vector3 direction = rotation * transform.forward;

            Vector3 pointOnArc = origin + direction * viewDistance;

            points[i + 1] = pointOnArc;
        }

        points[segments + 2] = origin;

        fovRenderer.SetPositions(points);

        if (fovRenderer.material != null)
        {
            fovRenderer.material.color = chasing ? Color.red : Color.yellow;
        }
    }

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