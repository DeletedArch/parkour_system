using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerCamera;

    private InputSystem controls;
    private Vector2 moveDirection = Vector2.zero;

    void Awake()
    {
        controls = new InputSystem();
    }

    void OnEnable()
    {
        controls.PlayerMovement.Enable();
    }

    void OnDisable()
    {
        controls.PlayerMovement.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to action events
        controls.PlayerMovement.Jump.performed += ctx => { Jump(); Debug.Log("Jump!"); };
        controls.PlayerMovement.Move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        controls.PlayerMovement.Move.canceled += ctx => moveDirection = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // controls.PlayerMovement.Move.ReadValue<Vector2>();
        Move(moveDirection);
        if (IsGrounded())
        {
            rb.drag = 5f;
        } else {
            rb.drag = 0f;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    void Move(Vector2 direction)
    {
        float moveX = (IsGrounded() ? 1 : 0.05f) * direction.y * speed * Time.deltaTime;
        float moveZ = (IsGrounded() ? 1 : 0.05f) * direction.x * speed * Time.deltaTime;
        Vector3 frontCam = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;
        rb.AddForce(frontCam * (moveX * speed * Time.deltaTime), ForceMode.VelocityChange);
        rb.AddForce(playerCamera.right * (moveZ * speed * Time.deltaTime), ForceMode.VelocityChange);
    }

    void Jump()
    {
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

}
