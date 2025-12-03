using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(CharacterController))]

public class WanderingAI : MonoBehaviour

{

    [Header("Movimiento")]

    public float speed = 3.0f;

    public float rotationSpeed = 5.0f;

    public float reachDistance = 0.5f;

    public float chaseSpeed = 6.0f;



    [Header("Waypoints")]

    public Transform[] waypoints;



    private GameObject player;

    private int currentWaypoint = 0;

    private int dirSign = 1; // +1 = adelante, -1 = atrás

    private CharacterController controller;



    private bool chasing = false;

    private bool patrolling = false;



    private Animator animator;





    void Start()

    {

        controller = GetComponent<CharacterController>();

        player = GameObject.FindWithTag("Player"); // busca el jugador por tag "Player"

        animator = GetComponent<Animator>();

    }



    void Update()

    {

        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;



        // Detecta al jugador si está al frente

        if (Physics.SphereCast(ray, 1.5f, out hit, 30f)) // límite de 10 metros

        {

            if (hit.collider.gameObject == player)

            {

                chasing = true;

                patrolling = false;



            }

        }

        else

        {

            // Si no ve al jugador, vuelve a patrullar

            chasing = false;

            patrolling = true;



        }



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







    }

    void OnControllerColliderHit(ControllerColliderHit hit)

    {

        if (hit.gameObject == player)

        {

            Debug.Log("¡El enemigo atrapó al jugador!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);


            // Aquí puedes añadir la lógica de daño, reiniciar nivel, etc.

        }

    }



    // Si usas colliders con triggers (por ejemplo, una zona de detección)

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



        if (direction.magnitude > 1.0f) // mientras esté lejos

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