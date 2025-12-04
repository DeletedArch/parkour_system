using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] public Transform playerCamera;
    [SerializeField] private Transform playerModel;

    [Header("Physics Settings")]
    [SerializeField] bool useCustomGravity = true;
    [SerializeField] bool canMove = true;
    [SerializeField] float gravity = -25f;
    [SerializeField] float risingMultiplier = 1f;
    [SerializeField] float apexMultiplier = 0.6f;
    [SerializeField] float fallingMultiplier = 2.5f;
    [SerializeField] float apexThreshold = 0.5f;  // Velocity range for apex
    [SerializeField] private float MaxVelocity = 12f;
    [SerializeField] private float sprintSpeed = 145f;
    [SerializeField] private float walkSpeed = 100f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airMoveSpeedMultiplier = 0.05f;
    [SerializeField] float jumpForwardPush = 10f;

    [Header("Physics Running Values")]
    [SerializeField] private float speed = 100f;
    [SerializeField] float customDrag = 0.4f;
    [SerializeField] internal RaycastHit RightRaycast;
    [SerializeField] internal RaycastHit LeftRaycast;

    private InputSystem controls;
    private Vector2 moveDirection = Vector2.zero;
    private StateMachine<PlayerController> _stateMachine;
    private bool isSprinting = false;
    private bool queueJump = false;

    internal Vector2 MoveDirection { get { return moveDirection; } }
    internal bool IsSprinting { get { return isSprinting; } }
    internal float groundDrag { get { return customDrag; } set { customDrag = value; } }
    internal Vector3 Velocity { get { return rb.velocity; } set { rb.velocity = value; } }
    internal bool UseCustomGravity { get { return useCustomGravity; } set { useCustomGravity = value; } }
    internal bool CanMove { get { return canMove; } set { canMove = value; } }

    void Awake()
    {
        _stateMachine = new StateMachine<PlayerController>(this);
        _stateMachine.AddState(new PlayerIdleState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerMoveState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerJumpState(GetComponent<Animator>()));
        _stateMachine.AddState(new PlayerLandState(GetComponent<Animator>()));
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
        controls.PlayerMovement.Jump.performed += ctx =>
        {
            if (!IsGrounded())
            {
                queueJump = true;
            }
            else
            {
                Jump();
            }
        };
        controls.PlayerMovement.Move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        controls.PlayerMovement.Move.canceled += ctx => moveDirection = Vector2.zero;
        controls.PlayerMovement.Sprint.performed += ctx => isSprinting = true;
        controls.PlayerMovement.Sprint.canceled += ctx => isSprinting = false;
        Debug.Log(Vector3.up * (gravity * risingMultiplier));
    }


    void ApplyCustomGravity()
    {
        if (!useCustomGravity) return;
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
        Vector3 gravityForce = Vector3.up * (gravity * multiplier);
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }

    void ApplyCustomDrag()
    {
        if (Math.Abs(rb.velocity.x) < 0.1f && Math.Abs(rb.velocity.z) < 0.1f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }
        float dragValue = customDrag;
        rb.velocity = new Vector3(rb.velocity.x * (1 - dragValue), rb.velocity.y, rb.velocity.z * (1 - dragValue));

    }

    void MaxVelocityUpdate()
    {
        if (rb.velocity.magnitude > MaxVelocity)
        {
            Vector3 limitedVelocity = rb.velocity.normalized * MaxVelocity;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    void FixedUpdate()
    {
        _stateMachine.Update();
        ApplyCustomGravity();
        ApplyCustomDrag();
        Move(moveDirection);

    }

    void Update()
    {
        Rotate(moveDirection);
        MaxVelocityUpdate();
        if (queueJump && IsGrounded())
        {
            Jump();
            queueJump = false;
        }
        if ((CloseToWallRight() || CloseToWallLeft()) && !IsGrounded() && moveDirection.y > 0f && moveDirection.x != 0f && rb.velocity.y < 0f)
        {
            _stateMachine.SetState<PlayerWallRunState>();
        } else
        {
            Debug.Log("Not close to wall" + CloseToWallLeft() + CloseToWallRight());
            Debug.Log("Move Dir" + moveDirection);
            Debug.Log("Is Grounded" + IsGrounded());
        }
    }

    public bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * 1.2f, Color.red);
        return Physics.Raycast(transform.position, Vector3.down, 1.2f, groundLayer);
    }

    public bool CloseToWallRight()
    {   
        Debug.DrawRay(transform.position, playerModel.right * 2f, Color.blue);
        return Physics.Raycast(transform.position, playerModel.right, out RightRaycast, 2f, wallLayer);

    }

    public bool CloseToWallLeft()
    {
        Debug.DrawRay(transform.position, -playerModel.right * 2f, Color.green);
        return Physics.Raycast(transform.position, -playerModel.right, out LeftRaycast, 2f, wallLayer);
    }

    public void UpdateSpeed()
    {
        speed = isSprinting ? sprintSpeed : walkSpeed;
    }

    public void AddForce(Vector3 force, ForceMode mode)
    {
        rb.AddForce(force, mode);
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
        if (!canMove) return;
        float moveX = direction.y * (speed * (IsGrounded() && !_stateMachine.CheckState<PlayerJumpState>() ? 1 : airMoveSpeedMultiplier));
        float moveZ = direction.x * (speed * (IsGrounded() && !_stateMachine.CheckState<PlayerJumpState>() ? 1 : airMoveSpeedMultiplier));
        Vector3 frontCam = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;
        rb.AddForce(frontCam * moveX, ForceMode.VelocityChange);
        rb.AddForce(playerCamera.right * moveZ, ForceMode.VelocityChange);

    }

    void Rotate(Vector2 direction)
    {
        Vector3 groundVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Quaternion targetRotation = groundVelocity != Vector3.zero ? Quaternion.LookRotation(groundVelocity) : playerModel.rotation;
        if (targetRotation != null && targetRotation != playerModel.rotation && direction != Vector2.zero)
        {
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, 0.1f);
        }
    }

    void Jump()
    {

        _stateMachine.SetState<PlayerJumpState>();
        Vector3 appliedJumpForce = Vector3.up * jumpForce * (speed == sprintSpeed ? 1.2f : 1f);
        if (moveDirection.magnitude > 0f)
        {
            float moveX = moveDirection.y;
            float moveZ = moveDirection.x;
            Vector3 frontJumpDirection = new Vector3(playerCamera.forward.x, 0f, playerCamera.forward.z).normalized * moveX * jumpForwardPush;
            Vector3 rightJumpDirection = playerCamera.right.normalized * moveZ * jumpForwardPush;
            rb.AddForce(frontJumpDirection, ForceMode.Impulse);
            rb.AddForce(rightJumpDirection, ForceMode.Impulse);
        }
        rb.AddForce(appliedJumpForce, ForceMode.Impulse);
    }

}
