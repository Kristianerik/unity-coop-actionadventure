using UnityEngine;

public class RangedEnemy : EnemyBase
{
    
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float preferredRange = 8f;
    [SerializeField] private float minRange = 4f;

    public GameObject ProjectilePrefab => projectilePrefab;
    public Transform FirePoint => firePoint;
    public float PreferredRange => preferredRange;
    public float MinRange => minRange;

    protected override void InitializeStates()
    {
        base.InitializeStates();
        // Override chase and attack states with ranged versions
        ChaseState = new RangedChaseState(this);
        AttackState = new RangedAttackState(this);
    }

    protected override void OnDamageTaken(Vector3 Knockback)
    {
        if (Health.IsDead()) return;

        if (StateMachine.CurrentState == IdleState || StateMachine.CurrentState == PatrolState)
        {
            StateMachine.ChangeState(ChaseState);
            return;
        }

        // Ranged enemies have higher dodge chances
        if (Random.value < 0.5f)
        {
            StateMachine.ChangeState(DodgeState);
        }
    }
}
