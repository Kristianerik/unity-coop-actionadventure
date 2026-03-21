using System.Collections.Generic;
using Mono.Cecil.Cil;
using NUnit.Framework;
using UnityEngine;

public class BossBase : EnemyBase
{
    
    [Header("Boss Settings")]
    [SerializeField] protected float phase2HealthThreshold = 0.5f;
    [SerializeField] private float enrageSpeedMultiplier = 1.5f;
    [SerializeField] private float enrageDamageMultiplier = 1.5f;

    [Header("Minion Spawning")]
    [SerializeField] private GameObject[] phase1MinionPrefabs;
    [SerializeField] private GameObject[] phase2MinionPrefabs;
    [SerializeField] private int minionsPerSummon = 3;
    [SerializeField] private float summonCooldown = 20f;

    [Header("Melee")]
    [SerializeField] private HitboxController meleeHitbox;


    [Header("Projectiles")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("weak Points")]
    [SerializeField] private List<WeakPoint> weakPoints = new List<WeakPoint>();

    [Header("Arena Hazards")]
    [SerializeField] private List<ArenaHazard> arenaHazards = new List<ArenaHazard>();
    [SerializeField] private LayerMask playerLayer;

    // Boss States
    protected BossStateCharge ChargeState;
    protected BossStateSlam SlamState;
    protected BossStateProjectileBarrage BarrageState;
    protected BossStateSummon SummonState;

    // State
    protected bool IsEnraged = false;
    protected int CurrentPhase = 1;
    private float _summonCooldownTimer = 0f;
    private float _specialAttackTimer = 0f;
    private float _specialAttackInterval = 8f;

    // Events
    public event System.Action<int> OnPhaseChanged;
    public event System.Action OnEnraged;

    public HitboxController MeleeHitbox => meleeHitbox;

    protected override void InitializeStates()
    {
        base.InitializeStates();

        // Boss never patrols - replace patrol with idle
        PatrolState = new EnemyStateIdle(this);
        
        // Initialize boss specific states
        ChargeState = new BossStateCharge(this);
        SlamState = new BossStateSlam(this);
        SlamState.SetPlayerLayer(playerLayer);
        BarrageState = new BossStateProjectileBarrage(this, projectilePrefab, firePoint);
        SummonState = new BossStateSummon(this, phase1MinionPrefabs, minionsPerSummon);
    }

    public override void Awake()
    {
        base.Awake();
        meleeHitbox?.Initialize(gameObject);
    }

    public override void Start()
    {
        base.Start();
        Health.OnHealthChanged += CheckPhaseTransition;

        // Initialize weak points
        foreach (var wp in weakPoints)
        {
            wp.Initialize(Health);
        }
    }

    protected override void Update()
    {
        base.Update();

        // Track special attack and summon timers
        if (_summonCooldownTimer > 0f)
        {
            _summonCooldownTimer -= Time.deltaTime;
        }

        _specialAttackTimer += Time.deltaTime;

        // Trigger special attacks periodically
        if (_specialAttackTimer >= _specialAttackInterval)
        {
            _specialAttackTimer = 0f;
            TriggerSpecialAttack();
        }
    }

    private void TriggerSpecialAttack()
    {
        if (!Aggro.HasTarget()) return;
        if (Health.IsDead()) return;

        if (CurrentPhase == 1)
        {
            TriggerPhase1Special();
        }
        else
        {
            TriggerPhase2Special();
        }
    }

    private void TriggerPhase1Special()
    {
        // Phase 1 - randomly choose charge, slam or summon
        int random = Random.Range(0, 3);
        switch(random)
        {
            case 0:
                ChangeState(ChargeState);
                Debug.Log("Executing Charge!");
                break;
            case 1:
                ChangeState(SlamState);
                Debug.Log("Executing Slam!");
                break;
            case 2:
            if (_summonCooldownTimer <= 0f)
                {
                    _summonCooldownTimer = summonCooldown;
                    ChangeState(SummonState);
                }
                break;
        }
    }

    private void TriggerPhase2Special()
    {
        // Phase 2 - barrage and summon ranged minions
        int random = Random.Range(0, 2);
        switch (random)
        {
            case 0:
                ChangeState(BarrageState);
                Debug.Log("Executing Barrage!");
                break;
            case 1:
                if (_summonCooldownTimer <= 0f)
                {
                    _summonCooldownTimer = summonCooldown;
                    // Update summon state with phase 2 minions 
                    SummonState = new BossStateSummon(this, phase2MinionPrefabs, minionsPerSummon);
                    ChangeState(SummonState);
                }
                break;
        }
    }

    private void CheckPhaseTransition(float current, float max)
    {
        float percent = current / max;

        Debug.Log($"Health: {current}/{max} | Percent: {percent} | CurrentPhase: {CurrentPhase}");

        if (percent <= phase2HealthThreshold && CurrentPhase < 2)
        {
            CurrentPhase = 2;
            OnPhaseChanged?.Invoke(2);
            EnterPhase2();
        }
    }

    protected virtual void EnterPhase2()
    {
        Debug.Log($"{gameObject.name} entered Phase2!");

        // Speed boost
        Agent.speed *= 1.2f;

        // Activate weak points
        foreach (var wp in weakPoints)
        {
            wp.SetActive(true);
        }

        // Activate arena hazards
        foreach (var hazard in arenaHazards)
        {
            hazard.Activate();
        }

        // Update special attack interval - more aggressive
        _specialAttackInterval = 5f;
    }

    protected virtual void TriggerEnrage()
    {
        if (IsEnraged) return;
        IsEnraged = true;

        Agent.speed *= enrageSpeedMultiplier;
        damageMultiplier = enrageDamageMultiplier;
        OnEnraged?.Invoke();
        Debug.Log($"{gameObject.name} is ENRAGED!");
    }

    // Called by child classes to add mroe phases
    protected virtual void EnterPhase(int phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
        Debug.Log($"{gameObject.name} entered Phase {phase}!");
    }

    public int GetCurrentPhase() => CurrentPhase;
    public bool GetIsEnraged() => IsEnraged;
}
