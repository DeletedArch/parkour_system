using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public enum WallRunDirection
{
    Right,
    Left
}

public class PlayerIdleState : State<PlayerController>
{
    private Animator _animator;

    public PlayerIdleState(Animator animator)
    {
        _animator = animator;
    }

    public override void Enter()
    {
        if (_owner.MoveDirection.magnitude >= 0.1f)
        {
            _stateMachine.SetState<PlayerMoveState>();
        }
    }

    public override void Update()
    {
        if (_owner.MoveDirection.magnitude >= 0.1f)
        {
            _stateMachine.SetState<PlayerMoveState>();
        }
    }

    public override void Exit()
    {

    }
}

public class PlayerMoveState : State<PlayerController>
{
    private Animator _animator;

    public PlayerMoveState(Animator animator)
    {
        _animator = animator;
    }

    public override void Enter()
    {
        if (_owner.MoveDirection.magnitude < 0.1f)
        {
            _stateMachine.SetState<PlayerIdleState>();
        }
        if (_owner.IsSprinting)
        {
            _stateMachine.SetState<PlayerSprintState>();
        } else
            _owner.UpdateSpeed();
    }

    public override void Update()
    {
        if (_owner.MoveDirection.magnitude < 0.1f)
        {
            _stateMachine.SetState<PlayerIdleState>();
        }
        if (_owner.IsSprinting)
        {
            _stateMachine.SetState<PlayerSprintState>();
        }
    }

    public override void Exit()
    {

    }
}

public class PlayerSprintState : State<PlayerController>
{
    private Animator _animator;

    public PlayerSprintState(Animator animator)
    {
        _animator = animator;
    }

    public override void Enter()
    {
        if (_owner.MoveDirection.magnitude < 0.1f)
        {
            _stateMachine.SetState<PlayerIdleState>();
        }
        if (!_owner.IsSprinting)
        {
            _stateMachine.SetState<PlayerMoveState>();
        } else
            _owner.UpdateSpeed();
        
    }

    public override void Update()
    {
        if (_owner.MoveDirection.magnitude < 0.1f)
        {
            _stateMachine.SetState<PlayerIdleState>();
        }
        if (!_owner.IsSprinting)
        {
            _stateMachine.SetState<PlayerMoveState>();
        }
    }

    public override void Exit()
    {

    }
}

public class PlayerJumpState : State<PlayerController>
{
    private Animator _animator;

    public PlayerJumpState(Animator animator)
    {
        _animator = animator;
    }

    public override void Enter()
    {
        Debug.Log("Entered Jump State");
    }

    public async override void Update()
    {
        _owner.groundDrag = 0.1f;
        await Task.Delay(200);
        if (_owner.IsGrounded())
        {
            _stateMachine.SetState<PlayerLandState>();
        }
    }

    public override void Exit()
    {

    }
}

public class PlayerLandState : State<PlayerController>
{
    private Animator _animator;

    public PlayerLandState(Animator animator)
    {
        _animator = animator;
    }

    public override void Enter()
    {
        // _owner.groundDrag = 2f;
        // Vector3 newForce = new Vector3(_owner.playerCamera.forward.x, 0f, _owner.playerCamera.forward.y).normalized;
        // _owner.AddForce(newForce * _owner.MoveDirection.y * _owner.LandPushOffForce, ForceMode.VelocityChange);
        // _owner.AddForce(_owner.playerCamera.right * _owner.MoveDirection.x * _owner.LandPushOffForce, ForceMode.VelocityChange);
        // trying to retain current force and prevent sudden stop
        //  Vector3 horizontalVelocity = new Vector3(_owner.Velocity.x, 0f, _owner.Velocity.z);
        //  Vector3 landingForce = horizontalVelocity.normalized * _owner.LandPushOffForce;
        //  _owner.AddForce(landingForce, ForceMode.VelocityChange);
        // await Task.Delay(200);
        // _owner.groundDrag = 5f;
        // _stateMachine.SetState<PlayerIdleState>();
        // fixed with adding physics materal
        // no delay for now as there's no need
        // this will be used later as an early jump trigger if key was queued
        _owner.groundDrag = 0.2f;
        _stateMachine.SetState<PlayerIdleState>();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {

    }
}

public class PlayerSlideState : State<PlayerController>
{
    private Animator _animator;

    public PlayerSlideState(Animator animator)
    {
        _animator = animator;
    }

    public override void Enter()
    {
    }

    public override void Update()
    {

    }

    public override void Exit()
    {

    }
}

public class PlayerWallRunState : State<PlayerController>
{
    private Animator _animator;
    private bool canCancel = false;
    private WallRunDirection wallRunDirection;
    private RaycastHit wallRaycast;
    private Vector3 normalVector;
    private Vector3 wallRunDirectionVector;
    private float wallRunSpeed = 13f;

    public PlayerWallRunState(Animator animator)
    {
        _animator = animator;
    }

    Task waitForCancel()
    {
        return Task.Run(async () =>
        {
            await Task.Delay(500);
            canCancel = true;
        });
    }

    public override void Enter()
    {
        _owner.UseCustomGravity = false;
        _owner.CanMove = false;
        if (_owner.CloseToWallRight())
        {
            wallRunDirection = WallRunDirection.Right;
        }
        else if (_owner.CloseToWallLeft())
        {
            wallRunDirection = WallRunDirection.Left;
        }
        wallRaycast = (wallRunDirection == WallRunDirection.Right) ? _owner.RightRaycast : _owner.LeftRaycast;
        normalVector = wallRaycast.normal;
        wallRunDirectionVector = (wallRunDirection == WallRunDirection.Right) ? Vector3.Cross(-normalVector, Vector3.up) : Vector3.Cross(normalVector, Vector3.up);
        _owner.Velocity = wallRunDirectionVector * wallRunSpeed - Vector3.up * 2f;
        waitForCancel();
    }

    public override void Update()
    {
        _owner.Velocity = wallRunDirectionVector * wallRunSpeed - Vector3.up * 2f;
        if (canCancel)
        {
            if ( !_owner.CloseToWallRight() && wallRunDirection == WallRunDirection.Right)
            {
                _stateMachine.SetState<PlayerIdleState>();
            }
            else if ( !_owner.CloseToWallLeft() && wallRunDirection == WallRunDirection.Left)
            {
                _stateMachine.SetState<PlayerIdleState>();
            } else if (_owner.IsGrounded())
            {
                _stateMachine.SetState<PlayerIdleState>();
            }
        }
    }

    public override void Exit()
    {
        _owner.UseCustomGravity = true;
        _owner.CanMove = true;
    }
}