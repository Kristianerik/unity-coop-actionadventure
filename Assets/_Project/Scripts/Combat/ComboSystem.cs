using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [Header("Combo Chains")]
    [SerializeField] private List<ComboChain> comboChains = new List<ComboChain>();

    [Header("References")]
    [SerializeField] private HitboxController meleeHitbox;
    [SerializeField] private Animator animator;

    // State
    private int _currentComboIndex = 0;
    private ComboChain _currentChain;
    private bool _isAttacking = false;
    private bool _comboWindowOpen = false;
    private float _attackTimer = 0f;
    private bool _hitboxActive = false;

    // Queued input
    private bool _nextAttackQueued = false;
    private AttackType _queuedAttackType;
    private Vector2 _queuedMoveInput;

    // Events
    public event System.Action<ComboAttack> OnAttackStarted;
    public event System.Action OnComboEnded;

    public void Initialize(HitboxController hitbox, Animator anim)
    {
        meleeHitbox = hitbox;
        animator = anim;
    }

    private void Update()
    {
        HandleAttackTimer();
    }

    // Public Attack Requests

    public void RequestLightAttack(Vector2 moveInput)
    {
        RequestAttack(AttackType.Light, moveInput);
    }

    public void RequestHeavyAttack(Vector2 moveInput)
    {
        RequestAttack(AttackType.Heavy, moveInput);
    }

    private void RequestAttack(AttackType type, Vector2 moveInput)
    {
        if (!_isAttacking)
        {
            StartCombo(type, moveInput);
        }
        else if (_comboWindowOpen)
        {
            // Queue the next attack
            _nextAttackQueued = true;
            _queuedAttackType = type;
            _queuedMoveInput = moveInput;
        }
    }

    // Combo Logic 

    private void StartCombo(AttackType type, Vector2 moveInput)
    {
        _currentChain = GetBestComboChain(type, moveInput);
        if (_currentChain == null || _currentChain.attacks.Count == 0) return;

        _currentComboIndex = 0;
        _isAttacking = true;
        ExecuteAttack(_currentChain.attacks[0]);
    }

    private ComboChain GetBestComboChain(AttackType type, Vector2 moveInput)
    {
        ComboChain bestChain = null;
        float bestScore = -1f;

        foreach (var chain in comboChains)
        {
            if (chain.attacks.Count == 0) continue;

            // Check if first attack matches input type
            if (!chain.MatchesNextInput(0, type)) continue;

            // Score by direction match
            float dot = Vector2.Dot(
                moveInput.normalized,
                chain.attacks[0].requiredDirection.normalized
            );

            if (dot > bestScore)
            {
                bestScore = dot;
                bestChain = chain;
            }
        }

        return bestChain;
    }

    private void ExecuteAttack(ComboAttack attack)
    {
        _attackTimer = 0f;
        _comboWindowOpen = false;
        _nextAttackQueued = false;
        _hitboxActive = false;

        meleeHitbox?.SetDamage(attack.damage);
        meleeHitbox?.SetKnockback(attack.knockbackForce);

        if (!string.IsNullOrEmpty(attack.animationTrigger))
            animator?.SetTrigger(attack.animationTrigger);

        OnAttackStarted?.Invoke(attack);
        Debug.Log($"Executing: {attack.attackName} ({attack.requiredInput})");
    }

    private void HandleAttackTimer()
    {
        if (!_isAttacking) return;

        ComboAttack current = _currentChain.attacks[_currentComboIndex];
        _attackTimer += Time.deltaTime;

        // Open/close combo window
        _comboWindowOpen = _attackTimer >= current.comboWindowStart &&
                        _attackTimer < current.comboWindowEnd;

        // Activate hitbox only once when entering active frames
        float activeFramesEnd = current.duration * 0.5f;
        if (_attackTimer <= activeFramesEnd && !_hitboxActive)
        {
            _hitboxActive = true;
            meleeHitbox?.ActivateHitbox();
        }
        else if (_attackTimer > activeFramesEnd && _hitboxActive)
        {
            _hitboxActive = false;
            meleeHitbox?.DeactivateHitbox();
        }

        // Try to continue combo
        if (_attackTimer >= current.comboWindowEnd)
        {
            if (_nextAttackQueued)
                TryContinueCombo();
            else if (_attackTimer >= current.duration)
                EndCombo();
        }
    }

    private void TryContinueCombo()
    {
        int nextIndex = _currentComboIndex + 1;

        // Check if the queued input matches the next attack in the chain
        if (nextIndex < _currentChain.attacks.Count &&
            _currentChain.MatchesNextInput(nextIndex, _queuedAttackType))
        {
            _currentComboIndex = nextIndex;
            ExecuteAttack(_currentChain.attacks[_currentComboIndex]);
        }
        else
        {
            // Queued input doesn't match chain - end combo and start fresh
            EndCombo();
            StartCombo(_queuedAttackType, _queuedMoveInput);
        }
    }

    public void EndCombo()
    {
        _isAttacking = false;
        _comboWindowOpen = false;
        _nextAttackQueued = false;
        _currentComboIndex = 0;
        _currentChain = null;
        meleeHitbox?.DeactivateHitbox();
        OnComboEnded?.Invoke();
    }

    public bool IsAttacking() => _isAttacking;
    public bool IsComboWindowOpen() => _comboWindowOpen;
}
