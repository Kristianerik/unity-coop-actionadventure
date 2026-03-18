using UnityEngine;

public class EnemyStateAttack : EnemyStateBase
{

    protected float _attackTimer = 0f;
    protected float _attackDuration = 0.8f;
    private bool _hasDealtDamage = false;
    private bool _hitboxActive = false;

    public EnemyStateAttack(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        Enemy.Agent.isStopped = true;
        _attackTimer = 0f;
        _hasDealtDamage = false;
        _hitboxActive = false;
        Enemy.EnemyAnimator?.SetTrigger("Attack");

        // Face the target
        if (Enemy.Aggro.HasTarget())
        {
            Vector3 direction = (Enemy.Aggro.GetCurrentTarget().position - Enemy.transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Enemy.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    public override void Update()
    {
        _attackTimer += Time.deltaTime;
        float activeFramesEnd = _attackDuration * 0.5f;

        // Activate hitbox if melee enemy
        if (Enemy is MeleeEnemy meleeEnemy && meleeEnemy.MeleeHitbox != null)
        {
            if (_attackTimer <= activeFramesEnd && !_hitboxActive)
            {
                _hitboxActive = true;
                meleeEnemy.MeleeHitbox.ActivateHitbox();
            }
            else if (_attackTimer > activeFramesEnd && _hitboxActive)
            {
                _hitboxActive = false;
                meleeEnemy.MeleeHitbox.DeactivateHitbox();
            }
        }
        else 
        {
            // Fallback range check for enemies without hitbox
            if (!_hasDealtDamage && _attackTimer >= _attackDuration * 0.5f) 
            {
                _hasDealtDamage = true;
                TryDealDamage();
            }
        }

        // Return to chase state affer attack completed
        if (_attackTimer >= _attackDuration)
        {
            Enemy.AttackCooldownTimer = Enemy.AttackCooldown;
            Enemy.ChangeState(Enemy.ChaseState);
        }
    }

    private void TryDealDamage()
    {
        if (!Enemy.Aggro.HasTarget()) return;
        Transform target = Enemy.Aggro.GetCurrentTarget();

        if (Vector3.Distance(Enemy.transform.position, target.position) <= Enemy.AttackRange)
        {
            HealthSystem health = target.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                Vector3 knockback = (target.position - Enemy.transform.position).normalized * Enemy.AttackKnockback;
                health.TakeDamage(Enemy.AttackDamage, knockback, Enemy.gameObject);
            }
        }
    }

    public override void Exit()
    {
        // Make sure hitbox is deactivated
        if (Enemy is MeleeEnemy meleeEnemy)
        {
            meleeEnemy.MeleeHitbox?.DeactivateHitbox();
        }
        Enemy.Agent.isStopped = false;
    }
    
}
