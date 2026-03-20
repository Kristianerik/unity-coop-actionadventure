using UnityEngine;

public class BossStateCharge : EnemyStateBase
{
    
    private float _chargeTimer = 0f;
    private float _chargeDuration = 0.8f;
    private float _chargeSpeed =15f;
    private Vector3 _chargeDirection;
    private bool _hasHit = false;

    public BossStateCharge(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        _chargeTimer = 0f;
        _hasHit = false;
        Enemy.Agent.isStopped = true;
        Enemy?.EnemyAnimator?.SetTrigger("Charge");

        if (Enemy.Aggro.HasTarget())
        {
            _chargeDirection = (Enemy.Aggro.GetCurrentTarget().position - Enemy.transform.position).normalized;
            _chargeDirection.y = 0;

            // Face charge direction
            Enemy.transform.rotation = Quaternion.LookRotation(_chargeDirection);
        }
    }

    public override void Update()
    {
        _chargeTimer += Time.deltaTime;

        // Move in charge direction
        Enemy.Agent.Move(_chargeDirection * _chargeSpeed * Time.deltaTime);

        // Deal damage during charge
        if (!_hasHit && Enemy.Aggro.HasTarget())
        {
            float distanceToTarget = Vector3.Distance(Enemy.transform.position, Enemy.Aggro.GetCurrentTarget().position);

            if (distanceToTarget <= Enemy.AttackRange)
            {
                _hasHit = true;
                HealthSystem health = Enemy.Aggro.GetCurrentTarget().GetComponentInParent<HealthSystem>();
                if (health != null)
                {
                    Vector3 knockback = _chargeDirection * 15f;
                    health.TakeDamage(Enemy.AttackDamage * 1.5f, knockback, Enemy.gameObject);
                }
            }
        }

        if (_chargeTimer >= _chargeDuration)
        {
            Enemy.ChangeState(Enemy.ChaseState);
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }
}
