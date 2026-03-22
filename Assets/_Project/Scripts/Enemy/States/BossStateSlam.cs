using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class BossStateSlam : EnemyStateBase
{
    
    private float _slamTimer = 0f;
    private float _slamDuration = 1.2f;
    private float _slamRadius = 4f;
    private bool _hasSlammed = false;
    private LayerMask _playerLayer;

    public BossStateSlam(EnemyBase enemy) : base(enemy) { }

    public void SetPlayerLayer(LayerMask layer) => _playerLayer = layer;

    public override void Enter()
    {
        _slamTimer = 0f;
        _hasSlammed = false;
        Enemy.Agent.isStopped = true;
        Enemy.EnemyAnimator?.SetTrigger("Slam");
    }

    public override void Update()
    {
        _slamTimer += Time.deltaTime;

        // Slame hits at midpoint of animation
        if (!_hasSlammed && _slamTimer >= _slamDuration * 0.5f)
        {
            _hasSlammed = true;
            PerformSlam();
        }

        if (_slamTimer >= _slamDuration)
        {
            Enemy.ChangeState(Enemy.ChaseState);
        }
    }

    private void PerformSlam()
    {
        // Hit all players in radius
        Collider[] hits = Physics.OverlapSphere(Enemy.transform.position, _slamRadius, _playerLayer);

        foreach (var hit in hits)
        {
            HealthSystem health = hit.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                Vector3 knockback = (hit.transform.position - Enemy.transform.position).normalized * 10f;
                knockback.y = 5f;
                health.TakeDamage(Enemy.AttackDamage * 2f, knockback, Enemy.gameObject);
            }
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }
}
