using Unity.VisualScripting;
using UnityEngine;

public class EnemyStateStunned : EnemyStateBase
{
    
    private float _stunTimer = 0f;
    private float _stunDuration = 1f;

    public EnemyStateStunned(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        _stunTimer = 0f;
        Enemy.Agent.isStopped = true;
        Enemy.EnemyAnimator?.SetTrigger("Stunned");
    }

    public override void Update()
    {
        _stunTimer += Time.deltaTime;
        if (_stunTimer >= _stunDuration)
        {
            if (Enemy.IsTargetDetected())
            {
                Enemy.ChangeState(Enemy.ChaseState);
            }
            else
            {
                Enemy.ChangeState(Enemy.IdleState);
            }
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }

}
