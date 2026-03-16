using UnityEngine;

public abstract class EnemyStateBase
{
    
    protected EnemyBase Enemy;

    public EnemyStateBase(EnemyBase enemy)
    {
        Enemy = enemy;
    }

    public virtual void Enter() {}
    public virtual void Update() {}
    public virtual void FixedUpdate() {}
    public virtual void Exit() {}

}
