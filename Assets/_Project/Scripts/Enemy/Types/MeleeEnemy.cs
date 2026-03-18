using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    
    [Header("Melee Settings")]
    [SerializeField] private HitboxController meleeHitbox;

    public HitboxController MeleeHitbox => meleeHitbox;

    protected override void InitializeStates()
    {
        base.InitializeStates();
    }

    public override void Awake()
    {
        base.Awake();
        meleeHitbox?.Initialize(gameObject);
    }

    protected override void OnDamageTaken(Vector3 Knockback)
    {
        if (Health.IsDead()) return;

        // Melee enemies are agressive - lower dodge chance
        if (StateMachine.CurrentState == IdleState || StateMachine.CurrentState == PatrolState)
        {
            StateMachine.ChangeState(ChaseState);
            return;
        }

        if (Random.value < 0.15f)
        {
            StateMachine.ChangeState(DodgeState);
        }
        else
        {
            StateMachine.ChangeState(ChaseState);
        }
    }
}
