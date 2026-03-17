using UnityEngine;

public class EnemyStateIdle : EnemyStateBase
{

    private float _idleTimer;
    private float _idleDuration = 2f;

    public EnemyStateIdle(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        Enemy.Agent.isStopped = true;
        Enemy.EnemyAnimator.SetBool("isMoving", false);
        _idleTimer = 0f;
    }

    public override void Update()
    {
        _idleTimer += Time.deltaTime;

        // Check for player
        if (Enemy.IsTargetDetected() || Enemy.Aggro.IsInCombat())
        {
            Enemy.ChangeState(Enemy.ChaseState);
            return;
        }

        // Return to patrol after idle duration
        if (_idleTimer >= _idleDuration && Enemy.PatrolPoints?.Length > 0)
        {
            Enemy.ChangeState(Enemy.PatrolState);
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }
}
