using System;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    
    [Header("Ability Settings")]
    [SerializeField] protected string abilityName;
    [SerializeField] protected float cooldown = 5f;  
    [SerializeField] protected float castTime = 0.3f;

    protected float _cooldownTimer = 0f;
    protected bool _isCasting = false;
    protected GameObject _owner;
    protected Animator _animator;

    public virtual void Initialize(GameObject owner, Animator animator)
    {
        _owner = owner;
        _animator = animator;
    } 

    protected virtual void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
    }

    public virtual bool CanUse() => _cooldownTimer <= 0f && !_isCasting;

    public virtual void TryUse()
    {
        if (!CanUse()) return;
        StartCoroutine(CastAbility());
    }

    protected virtual System.Collections.IEnumerator CastAbility()
    {
        if (_owner == null)
        {
            Debug.LogError($"{abilityName} has no owner assigned! Make sure Initialize is called.");
            yield break;
        }
        
        _isCasting = true;

        // Play cast animation 
        _animator?.SetTrigger("CastAbility");

        // Wait for cast time
        yield return new WaitForSeconds(castTime);

        Execute();

        _cooldownTimer = cooldown;
        _isCasting = false; 
    }

    protected abstract void Execute();

    public float GetCooldown() => Mathf.Clamp01(_cooldownTimer/cooldown);
    public bool IsCasting() => _isCasting;
    public string GetAbilityName() => abilityName;
}
