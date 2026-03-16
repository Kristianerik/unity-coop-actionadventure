using UnityEngine;

public class EnemyStateDodge : EnemyStateBase
{
    
    private float _dodgeTimer = 0f;
    private float _dodgeDuration = 0.4f;
    private Vector3 _dodgeDirection;

    public EnemyStateDodge(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        _dodgeTimer = 0f;
        Enemy.Agent.isStopped = true;
        Enemy.EnemyAnimator?.SetTrigger("Dodge");

        // Dodge sideways relative to target
        if (Enemy.Aggro.HasTarget())
        {
            Vector3 toTarget = (Enemy.Aggro.GetCurrentTarget().position - Enemy.transform.position).normalized;
            _dodgeDirection = Vector3.Cross(toTarget, Vector3.up).normalized;

            // Random dodge to left or right
            if (Random.value > 0.5f)
            {
                _dodgeDirection = -_dodgeDirection;
            }
        }
        else
        {
            Enemy.ChangeState(Enemy.IdleState);
        }
    }

    public override void Update()
    {
        _dodgeTimer += Time.deltaTime;

        // Move in dodge direction
        Enemy.Agent.Move(_dodgeDirection * 5f * Time.deltaTime);

        if (_dodgeTimer >= _dodgeDuration)
        {
            if (Enemy.IsTargetDetected())
            {
                Enemy.ChangeState(Enemy.ChaseState);
            }
            else
            {
                Enemy.ChangeState(Enemy.IdleState);
            }
        }
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }

}
