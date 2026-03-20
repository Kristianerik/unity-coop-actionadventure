using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class BossStateProjectileBarrage : EnemyStateBase
{
    
    private float _barrageTimer = 0f;
    private float _barrageDuration = 3f;
    private float _fireInterval = 0.3f;
    private float _fireTimer = 0f;
    private GameObject _projectilePrefab;
    private Transform _firePoint;
    private int _projectilesPerBurst = 3;
    private float _spreadAngle = 30f;

    public BossStateProjectileBarrage(EnemyBase enemy, GameObject projectilePrefab, Transform firePoint) : base(enemy)
    {
        _projectilePrefab = projectilePrefab;
        _firePoint = firePoint;
    }

    public override void Enter()
    {
        _barrageTimer = 0f;
        _fireTimer = 0f;
        Enemy.Agent.isStopped = true;
        Enemy.EnemyAnimator?.SetTrigger("Barrage");
    }

    public override void Update()
    {
        _barrageTimer += Time.deltaTime;
        _fireTimer += Time.deltaTime;

        // Face target during barrage
        if (Enemy.Aggro.HasTarget())
        {
            Vector3 direction = (Enemy.Aggro.GetCurrentTarget().position - Enemy.transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Enemy.transform.rotation = Quaternion.Slerp(Enemy.transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
            }
        }
        
        // Fire burst every interval
        if (_fireTimer >= _fireInterval)
        {
            _fireTimer = 0f;
            FireBurst();
        }

        if (_fireTimer >= _fireInterval)
        {
            _fireTimer = 0f;
            FireBurst();
        }

        if(_barrageTimer >= _barrageDuration)
        {
            Enemy.ChangeState(Enemy.ChaseState);
        }
    }

    private void FireBurst()
    {
        if (_projectilePrefab == null || _firePoint == null) return;
        if (!Enemy.Aggro.HasTarget()) return;

        Vector3 baseDirection = (Enemy.Aggro.GetCurrentTarget().position - _firePoint.position).normalized;

        // Fire spread of projectiles
        for (int i = 0; i < _projectilesPerBurst; i++)
        {
            float angle = -_spreadAngle + (i * (_spreadAngle *2f / (_projectilesPerBurst - 1)));
            Vector3 spreadDirection = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;

            GameObject projectile = GameObject.Instantiate(_projectilePrefab, _firePoint.position, Quaternion.LookRotation(spreadDirection));

            if (projectile.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(Enemy.gameObject, Enemy.AttackDamage, 12f);
            }
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }
}
