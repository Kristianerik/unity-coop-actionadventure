using UnityEngine;

public class EnemyStateChase : EnemyStateBase
{

    public EnemyStateChase(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        Enemy.Agent.speed = 5f;
        Enemy.EnemyAnimator?.SetBool("IsMoving", true);
    }

    public override void Update()
    {
        // Only return to patrol if oput of range AND not in combat
        if (Enemy.HasLostTarget() && !Enemy.Aggro.IsInCombat())
        {
            Enemy.ChangeState(Enemy.PatrolState);
            return;
        }

        if (Enemy.IsInAttackRange() && Enemy.AttackCooldownTimer <= 0f)
        {
            Enemy.ChangeState(Enemy.AttackState);
            return;
        }

        // Keep chasing
        if (Enemy.Aggro.HasTarget())
        {
            Enemy.Agent.SetDestination(Enemy.Aggro.GetCurrentTarget().position);
        }
    }

    public override void Exit()
    {
        Enemy.Agent.ResetPath();
    }
    
}
