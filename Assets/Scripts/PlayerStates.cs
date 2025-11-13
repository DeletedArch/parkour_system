using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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
        await Task.Delay(200);
        if (_owner.IsGrounded())
        {
            _stateMachine.SetState<PlayerIdleState>();
        }
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