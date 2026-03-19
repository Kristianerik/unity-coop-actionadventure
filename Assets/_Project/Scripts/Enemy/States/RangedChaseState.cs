using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class RangedChaseState : EnemyStateChase
{
    
    private RangedEnemy _rangedEnemy;

    public RangedChaseState(RangedEnemy enemy) : base(enemy)
    {
        _rangedEnemy = enemy;
    }

    public override void Update()
    {
        if (!Enemy.Aggro.HasTarget()) return;

        if (Enemy.HasLostTarget() && !Enemy.Aggro.IsInCombat())
        {
            Enemy.ChangeState(Enemy.PatrolState);
            return;
        }

        float distanceToTarget = Vector3.Distance(Enemy.transform.position, Enemy.Aggro.GetCurrentTarget().position);

        // Target too close - retreat 
        if (distanceToTarget < _rangedEnemy.MinRange)
        {
            Vector3 awayFromTarget = (Enemy.transform.position - Enemy.Aggro.GetCurrentTarget().position).normalized;
            Enemy.Agent.SetDestination(Enemy.transform.position + awayFromTarget * 8f);
        }

        // Always shoot off cooldown when in attack range
        if (Enemy.AttackCooldownTimer <= 0f && distanceToTarget <= _rangedEnemy.AttackRange)
        {
            Enemy.ChangeState(Enemy.AttackState);
            return;
        }

        // Target too far - move closer
        if (distanceToTarget > _rangedEnemy.PreferredRange)
        {
            
            Enemy.Agent.SetDestination(Enemy.Aggro.GetCurrentTarget().position);
        }
    }
}
