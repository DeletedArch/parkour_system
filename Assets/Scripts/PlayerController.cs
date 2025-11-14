using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerModel;

    [Header("Physics Settings")]
    [SerializeField] float gravity = -25f;
    [SerializeField] float risingMultiplier = 1f;
    [SerializeField] float apexMultiplier = 0.6f;
    [SerializeField] float fallingMultiplier = 2.5f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float sprintSpeed = 145f;
    [SerializeField] private float walkSpeed = 100f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airMoveSpeedMultiplier = 0.05f;
    [SerializeField] private float customDrag = 0.4f;

    [Header("Apex Detection")]
    [SerializeField] float apexThreshold = 0.5f;  // Velocity range for apex

    private InputSystem controls;
    private Vector2 moveDirection = Vector2.zero;
    private StateMachine<PlayerController> _stateMachine;
    private bool isSprinting = false;

    public Vector2 MoveDirection { get { return moveDirection; } }
    public bool IsSprinting { get { return isSprinting; } }

    void Awake()
    {
        _stateMachine = new StateMachine<PlayerController>(this);
        _stateMachine.AddState(new PlayerIdleState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerMoveState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerJumpState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerSlideState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerSprintState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerWallRunState(GetComponent<Animator>()));
        _stateMachine.SetState<PlayerIdleState>();
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
        controls.PlayerMovement.Jump.performed += ctx => Jump();
        controls.PlayerMovement.Move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        controls.PlayerMovement.Move.canceled += ctx => moveDirection = Vector2.zero;
        controls.PlayerMovement.Sprint.performed += ctx => isSprinting = true;
        controls.PlayerMovement.Sprint.canceled += ctx => isSprinting = false;
        Debug.Log(Vector3.up * (gravity * risingMultiplier));
    }


    void ApplyCustomGravity()
    {
        float multiplier;

        // Determine which phase of jump
        if (rb.velocity.y > apexThreshold)
        {
            // Rising
            multiplier = risingMultiplier;
        }
        else if (rb.velocity.y > -apexThreshold)
        {
            // At apex (that floaty feeling)
            multiplier = apexMultiplier;
        }
        else
        {
            // Falling (heavier, more responsive)
            multiplier = fallingMultiplier;
        }

        // Apply the gravity force
        Vector3 gravityForce = Vector3.up * (gravity * multiplier) * Time.deltaTime;
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }

    void ApplyCustomDrag()
    {
        if (rb.velocity.magnitude < 0.1f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }
        float dragValue = customDrag * Time.deltaTime;
        rb.velocity = new Vector3(rb.velocity.x * (1 - dragValue), rb.velocity.y, rb.velocity.z * (1 - dragValue));
        
    }

    // Update is called once per frame
    void Update()
    {
        _stateMachine.Update();
        // Debug.Log("Move Direction: " + moveDirection + " | IsSprinting: " + isSprinting);
        ApplyCustomGravity();
        ApplyCustomDrag();
        Move(moveDirection);
        if (IsGrounded())
        {
            customDrag = 5f;
        }
        else
        {
            customDrag = 0.4f;
        }


    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    public bool CloseToWall()
    {
        return Physics.Raycast(transform.position, playerModel.right, 0.6f, groundLayer) ||
               Physics.Raycast(transform.position, -playerModel.right, 0.6f, groundLayer);
    }

    public void UpdateSpeed()
    {
        speed = isSprinting ? sprintSpeed : walkSpeed;
    }

    void Move(Vector2 direction)
    {
        // if (direction == Vector2.zero && IsGrounded())
        // {
        //     if (rb.velocity.magnitude > 0.1f)
        //         rb.AddForce(-rb.velocity * customDrag * Time.deltaTime, ForceMode.Acceleration);
        //     else
        //         rb.velocity = new Vector3(0, rb.velocity.y, 0);
        // }
        // else
        {
            float moveX = direction.y * (speed * (IsGrounded() ? 1 : airMoveSpeedMultiplier)) * Time.deltaTime;
            float moveZ = direction.x * (speed * (IsGrounded() ? 1 : airMoveSpeedMultiplier)) * Time.deltaTime;
            Vector3 frontCam = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;
            rb.AddForce(frontCam * moveX, ForceMode.VelocityChange);
            rb.AddForce(playerCamera.right * moveZ, ForceMode.VelocityChange);
            Vector3 groundVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Quaternion targetRotation = groundVelocity != Vector3.zero ? Quaternion.LookRotation(groundVelocity) : playerModel.rotation;
            if (targetRotation != playerModel.rotation && direction != Vector2.zero)
            {
                playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, 0.1f);
            }
        }
    }

    void Jump()
    {
        if (IsGrounded())
        {
            _stateMachine.SetState<PlayerJumpState>();
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

}
