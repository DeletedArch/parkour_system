using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

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
        _owner.groundDrag = 2f;
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
        _owner.groundDrag = 5f;
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

    public PlayerWallRunState(Animator animator)
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