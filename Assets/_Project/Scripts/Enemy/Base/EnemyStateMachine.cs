using UnityEngine;

public class EnemyStateMachine
{
    
    public EnemyStateBase CurrentState { get; private set; }
    private EnemyStateBase _previousState;

    public void Initialize(EnemyStateBase startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(EnemyStateBase newState)
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
