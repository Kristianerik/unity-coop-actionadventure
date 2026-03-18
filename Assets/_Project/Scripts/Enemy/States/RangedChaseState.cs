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

        // Target too close - back away
        if (distanceToTarget < _rangedEnemy.MinRange)
        {
            Vector3 awayFromTarget = (Enemy.transform.position - Enemy.Aggro.GetCurrentTarget().position).normalized;
            Enemy.Agent.SetDestination(Enemy.transform.position + awayFromTarget * 3f);
            return;
        }

        // In attack range - attack
        if (distanceToTarget <= _rangedEnemy.AttackRange && Enemy.AttackCooldownTimer <= 0f)
        {
            Enemy.ChangeState(Enemy.AttackState);
            return;
        }        

        // Too far - move to preferred range
        if (distanceToTarget > _rangedEnemy.PreferredRange)
        {
            Enemy.Agent.SetDestination(Enemy.Aggro.GetCurrentTarget().position);
            return;
        }

        // At preferred range - strafe sideways
        Vector3 toTarget = (Enemy.Aggro.GetCurrentTarget().position - Enemy.transform.position).normalized;
        Vector3 strafeDir = Vector3.Cross(toTarget, Vector3.up).normalized;
        if (Random.value > 0.5f) strafeDir = -strafeDir;
        Enemy.Agent.SetDestination(Enemy.transform.position + strafeDir * 2f);
    }
}
