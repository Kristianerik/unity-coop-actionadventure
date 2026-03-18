using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class RangedAttackState : EnemyStateAttack
{
    
    private RangedEnemy _rangedEnemy;
    private bool _hasFired = false;

    public RangedAttackState(RangedEnemy enemy) : base(enemy)
    {
        _rangedEnemy = enemy;
    }

    public override void Enter()
    {
        Enemy.Agent.isStopped = true;
        _hasFired = false;

        // Face target
        if (Enemy.Aggro.HasTarget())
        {
            Vector3 direction = (Enemy.Aggro.GetCurrentTarget().position - Enemy.transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Enemy.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        Enemy.EnemyAnimator?.SetTrigger("Attack");
    }

    public override void Update()
    {
        _attackTimer += Time.deltaTime;

        // Fire at midpoint of attack animation
        if (!_hasFired && _attackTimer >= _attackDuration * 0.5f)
        {
            _hasFired = true;
            FireProjectile();
        }

        if (_attackTimer >= _attackDuration)
        {
            Enemy.AttackCooldownTimer = Enemy.AttackCooldown;
            Enemy.ChangeState(Enemy.ChaseState);
        }
    }

    private void FireProjectile()
    {
        if (_rangedEnemy.ProjectilePrefab == null || _rangedEnemy.FirePoint == null) return;
        if (!_rangedEnemy.Aggro.HasTarget()) return;

        // Aim at target
        Vector3 direction = (_rangedEnemy.Aggro.GetCurrentTarget().position - _rangedEnemy.FirePoint.position).normalized;

        GameObject projectile = GameObject.Instantiate( _rangedEnemy.ProjectilePrefab, _rangedEnemy.FirePoint.position, Quaternion.LookRotation(direction));

        if (projectile.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Initialize(_rangedEnemy.gameObject, _rangedEnemy.AttackDamage, 15f);
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }
}
