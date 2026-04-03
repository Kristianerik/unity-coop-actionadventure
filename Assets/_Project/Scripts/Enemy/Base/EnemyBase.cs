using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AggroSystem))]
public class EnemyBase : MonoBehaviour, IResettable
{
    
    [Header("Detection")]
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float loseAggroRange = 15f;

    [Header("Combat")]
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float damageMultiplier = 1f;
    [SerializeField] protected float attackCoolDown = 1.5f;
    [SerializeField] protected float attackKnockback = 5f;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float chaseSpeed = 5f;

    [Header("Patrol")]
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float patrolWaitTime = 2f;

    // Components
    public NavMeshAgent Agent { get; private set; }
    public HealthSystem Health { get; private set; }
    public AggroSystem Aggro { get; private set; }
    public Animator EnemyAnimator { get; private set; }

    // State Machine
    public EnemyStateMachine StateMachine;

    // States
    public IEnemyState  IdleState { get; protected set; }
    public IEnemyState  PatrolState { get; protected set; }
    public IEnemyState  ChaseState { get; protected set; }
    public IEnemyState  AttackState { get; protected set; }
    public IEnemyState  DodgeState { get; protected set; }
    public IEnemyState  StunnedState { get; protected set; }
    public IEnemyState  DeatghState { get; protected set; }

    // Timers
    public float AttackCooldownTimer { get; set; }

    // Properties
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage * damageMultiplier;
    public float AttackKnockback => attackKnockback;
    public float AttackCooldown => attackCoolDown;
    public float DetectionRange => detectionRange;
    public float LoseAggroRange => loseAggroRange;
    public Transform[] PatrolPoints => patrolPoints;
    public float PatrolWaitTime => patrolWaitTime;

    // Saved state variables for checkpoint respawn
    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private float _savedHealth;
    private IEnemyState _savedState;
    private GameObject _prefabReference;


    public virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Health = GetComponent<HealthSystem>();
        Aggro = GetComponent<AggroSystem>();
        EnemyAnimator = GetComponentInChildren<Animator>();

        // Initialize state machine
        StateMachine = new EnemyStateMachine();
        InitializeStates();
    }

    protected virtual void InitializeStates()
    {
        IdleState = new EnemyStateIdle(this);
        PatrolState = new EnemyStatePatrol(this);
        ChaseState = new EnemyStateChase(this);
        AttackState = new EnemyStateAttack(this);
        DodgeState = new EnemyStateDodge(this);
        StunnedState = new EnemyStateStunned(this);
        DeatghState = new EnemyStateDeath(this);
    }

    public virtual void Start()
    {
        Agent.speed = moveSpeed;

        // Subscribe to health events
        Health.OnDeath += OnDeath;
        Health.OnDamageTaken += OnDamageTaken;

        // Start in idle or patrol
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            StateMachine.Initialize(PatrolState);
        } 
        else
        {
            StateMachine.Initialize(IdleState);
        }
    }

    protected virtual void Update()
    {
        if (AttackCooldownTimer > 0f)
        {
            AttackCooldownTimer -= Time.deltaTime;
        }

        StateMachine.Update();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    public void SaveState()
    {
        _savedPosition = transform.position;
        _savedRotation = transform.rotation;
        _savedHealth = Health.GetHealthPercent() * 100f;
        _savedState = StateMachine.CurrentState;
        Debug.Log($"{gameObject.name} state saved");
    }

    public void RestoreState()
    {
        // If this object was destroyed respawn it
        if (this == null || gameObject == null)
        {
            if (_prefabReference != null)
            {
                GameObject newEnemy = Instantiate(
                    _prefabReference,
                    _savedPosition,
                    _savedRotation
                );
                // Copy saved state to new instance
                EnemyBase newBase = newEnemy.GetComponent<EnemyBase>();
                if (newBase != null)
                {
                    newBase._savedPosition = _savedPosition;
                    newBase._savedHealth = _savedHealth;
                    newBase.RestoreState();
                }
            }
            return;
        }

        // Restore existing enemy
        transform.position = _savedPosition;
        transform.rotation = _savedRotation;
        Health.ResetHealth();
        if (_savedHealth < 100f)
            Health.TakeDamage(100f - _savedHealth, Vector3.zero);

        Agent.enabled = true;
        gameObject.SetActive(true);

        if (_savedState != null)
            StateMachine.ChangeState(_savedState);
        else
            StateMachine.Initialize(IdleState);
    }
 
    protected virtual void OnDamageTaken(Vector3 Knockback)
    {
        if (Health.IsDead()) return;

        // Always chase attacker if currently idle or patrolling
        if (StateMachine.CurrentState == IdleState || StateMachine.CurrentState == PatrolState)
        {
            StateMachine.ChangeState(ChaseState);
            return;
        }

        // Chance to dodge on taking damage
        if (!Health.IsDead() && Random.value < 0.3f)
        {
            StateMachine.ChangeState(DodgeState);
        }
    }

    public void OnDeath()
    {
        StateMachine.ChangeState(DeatghState);
    }

    public void ChangeState(IEnemyState newState)
    {
        StateMachine.ChangeState(newState);
    }

    public bool IsInAttackRange()
    {
        if (!Aggro.HasTarget()) return false;
        return Vector3.Distance(transform.position, Aggro.GetCurrentTarget().position) <= attackRange;
    }

    public bool IsTargetDetected()
    {
        if (!Aggro.HasTarget()) return false;
        return Vector3.Distance(transform.position, Aggro.GetCurrentTarget().position) <= detectionRange;
    }

    public bool HasLostTarget()
    {
        if (!Aggro.HasTarget()) return true;
        return Vector3.Distance(transform.position, Aggro.GetCurrentTarget().position) > loseAggroRange;
    }

    public void SetPrefabReference(GameObject prefab)
    {
        _prefabReference = prefab;
    }
}
