using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    private float currentSpeed;
    private bool isRunning;

    private CharacterController controller;
    [SerializeField] private Transform camera;
    public float rotSpeed = 15.0f;
    private Vector2 moveInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
    }

    // Movimiento (Input del joystick o teclado)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Correr (Input de botón)
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
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

        // Aplicar gravedad
        controller.Move(Physics.gravity * Time.deltaTime);
    }
}