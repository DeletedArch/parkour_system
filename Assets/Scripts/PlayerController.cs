using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float sprintSpeed = 145f;
    [SerializeField] private float walkSpeed = 100f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerModel;

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
    }

    // Update is called once per frame
    void Update()
    {
        _stateMachine.Update();
        // Debug.Log("Move Direction: " + moveDirection + " | IsSprinting: " + isSprinting);
        Move(moveDirection);
        if (IsGrounded())
        {
            rb.drag = 5f;
        }
        else
        {
            rb.drag = 0.4f;
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
        float moveX = (IsGrounded() ? 1 : 0.05f) * direction.y * speed * Time.deltaTime;
        float moveZ = (IsGrounded() ? 1 : 0.05f) * direction.x * speed * Time.deltaTime;
        Vector3 frontCam = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;
        rb.AddForce(frontCam * (moveX * speed * Time.deltaTime), ForceMode.VelocityChange);
        rb.AddForce(playerCamera.right * (moveZ * speed * Time.deltaTime), ForceMode.VelocityChange);
        Vector3 groundVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Quaternion targetRotation = groundVelocity != Vector3.zero ? Quaternion.LookRotation(groundVelocity) : playerModel.rotation;
        if (targetRotation != playerModel.rotation)
        {
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, 0.1f);
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
