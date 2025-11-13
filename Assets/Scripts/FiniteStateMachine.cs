using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class StateMachine<T>
{
    private T _owner;
    private Dictionary<System.Type, State<T>> _states;
    private State<T> _currentState;

    public StateMachine(T owner)
    {
        _owner = owner;
        _states = new Dictionary<System.Type, State<T>>();
    }

    public bool CheckState<TS>() where TS : State<T>
    {
        return _currentState.GetType() == typeof(TS);
    }

    public void Update()
    {
        if (_currentState != null)
            _currentState.Update();
            UnityEngine.Debug.Log("Current State: " + _currentState.GetType().ToString());
    }

    public void AddState(State<T> state)
    {
        state.SetState(this, _owner);
        _states[state.GetType()] = state;
    }

    public void SetState<TS>() where TS : State<T>
    {

        if (_currentState != null)
            _currentState.Exit();
        if (_states.ContainsKey(typeof(TS)))
        {
            _currentState = _states[typeof(TS)];
            _currentState.Enter();
        }
    }
}

public abstract class State<T>
{
    protected T _owner;
    protected StateMachine<T> _stateMachine;


    public virtual State<T> SetState(StateMachine<T> sm, T owner)
    {
        _stateMachine = sm;
        _owner = owner;
        return this;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}