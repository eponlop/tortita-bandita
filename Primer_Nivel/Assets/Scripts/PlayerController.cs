using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;
    [SerializeField] private Transform camera;
    public float rotSpeed = 15.0f;
    private Vector2 moveInput;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("Move: " + moveInput);
    }

    private void Update()
    {
        Vector3 horizVel = new Vector3(moveInput.x, 0, moveInput.y) * speed;
        if (!Mathf.Approximately(horizVel.magnitude, 0.0f))
        {
            Quaternion rot = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            horizVel = rot * horizVel;
            Quaternion direction = Quaternion.LookRotation(horizVel);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);

        }
        controller.Move(horizVel * Time.deltaTime);
    }
}
