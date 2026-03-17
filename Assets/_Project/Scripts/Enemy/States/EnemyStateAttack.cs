using UnityEngine;

public class EnemyStateAttack : EnemyStateBase
{

    private float _attackTimer = 0f;
    private float _attackDuration = 0.8f;
    private bool _hasDealtDamage = false;

    public EnemyStateAttack(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        Enemy.Agent.isStopped = true;
        _attackTimer = 0f;
        _hasDealtDamage = false;
        Enemy.EnemyAnimator?.SetTrigger("Attack");
    }

    public override void Update()
    {
        _attackTimer += Time.deltaTime;

        // Deal damage at midpoint of attack animation
        if (!_hasDealtDamage && _attackTimer >= _attackDuration * 0.5f)
        {
            _hasDealtDamage = true;
            TryDealDamage();
        }

        // Return to chase state affet attack completed
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
        Enemy.Agent.isStopped = false;
    }
    
}
