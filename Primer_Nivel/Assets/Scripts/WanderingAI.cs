using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WanderingAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 3.0f;
    public float rotationSpeed = 5.0f;
    public float reachDistance = 0.5f;

    [Header("Waypoints")]
    public Transform[] waypoints;

    private int currentWaypoint = 0;
    private int dirSign = 1; // +1 = adelante, -1 = atrás (antes 'direction' como int)
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Vector3 targetPos = waypoints[currentWaypoint].position;
        Vector3 dirToTarget = targetPos - transform.position; // antes llamada 'direction' (conflictiva)
        dirToTarget.y = 0f;

        float distance = dirToTarget.magnitude;

        // Si estamos cerca del waypoint, avanzamos al siguiente o invertimos la dirección
        if (distance < reachDistance)
        {
            currentWaypoint += dirSign;

            // Si llegamos al final o al inicio, invertimos el sentido
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = waypoints.Length - 2; // retrocede al penúltimo
                dirSign = -1;
            }
            else if (currentWaypoint < 0)
            {
                currentWaypoint = 1; // avanza al segundo
                dirSign = 1;
            }

            return;
        }

        // Rotar suavemente hacia el waypoint
        if (dirToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime * 100f
            );
        }

        // Mover hacia adelante (respetando colisiones)
        Vector3 move = transform.forward * speed * Time.deltaTime;
        controller.Move(move);
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
