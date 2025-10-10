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
    [SerializeField] private InputAction moveAction;

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
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    void Move(Vector2 direction)
    {
        // Debug.Log(direction);
        Vector3 move = new Vector3(direction.x, 0, direction.y);
        rb.AddForce(move * speed * Time.deltaTime);
    }

    void Jump()
    {
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

}
