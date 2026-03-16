using UnityEngine;

public class EnemyStateDeath : EnemyStateBase
{
    
    private float _deathTimer = 0f;
    private float _deathDuration = 3f;

    public EnemyStateDeath(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        Enemy.Agent.isStopped = true;
        Enemy.Agent.enabled = false;
        Enemy.EnemyAnimator?.SetTrigger("Death");

        // Disable colliders
        foreach (var col in Enemy.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }

    public override void Update()
    {
        _deathTimer += Time.deltaTime;
        if (_deathTimer >= _deathDuration)
        {
            GameObject.Destroy(Enemy.gameObject);
        }
    }
}
