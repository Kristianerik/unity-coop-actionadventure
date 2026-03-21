using UnityEngine;

public class EnemyStateMachine
{
    
    public IEnemyState CurrentState { get; private set; }
    private IEnemyState _previousState;

    public void Initialize(IEnemyState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(IEnemyState newState)
    {
        if (newState == CurrentState) return;

        _previousState = CurrentState;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void RevertToPreviousState()
    {
        if (_previousState != null)
        {
            ChangeState(_previousState);
        }
    }

    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.FixedUpdate();

}
