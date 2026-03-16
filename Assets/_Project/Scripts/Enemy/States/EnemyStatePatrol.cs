using UnityEngine;

public class EnemyStatePatrol : EnemyStateBase
{
    
    private int _currentPatrolIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;

    public EnemyStatePatrol(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        Enemy.Agent.speed = Enemy.Agent.speed;
        Enemy.EnemyAnimator.SetBool("isMoving", true);
        MoveToNextPoint();
    }

    public override void Update()
    {
        // Check for player furst
        if (Enemy.IsTargetDetected())
        {
            Enemy.ChangeState(Enemy.ChaseState);
            return;
        }   

        if (_isWaiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= Enemy.PatrolWaitTime)
            {
                _isWaiting = false;
                MoveToNextPoint();
            }
            return;
        }

        // Check if reached patrol point
        if (!Enemy.Agent.pathPending && Enemy.Agent.remainingDistance <= Enemy.Agent.stoppingDistance)
        {
            _isWaiting = true;
            _waitTimer = 0f;
            Enemy.EnemyAnimator.SetBool("isMoving", false);
            _currentPatrolIndex = (_currentPatrolIndex + 1) % Enemy.PatrolPoints.Length;
        }
    }


    private void MoveToNextPoint()
    {
        if (Enemy.PatrolPoints.Length == 0) return;
        Enemy.Agent.SetDestination(Enemy.PatrolPoints[_currentPatrolIndex].position);
        Enemy.EnemyAnimator.SetBool("isMoving", true);
    }
}
