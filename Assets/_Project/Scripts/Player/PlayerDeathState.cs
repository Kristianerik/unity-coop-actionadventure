using System.Collections;
using UnityEngine;

public class PlayerDeathState : MonoBehaviour
{
    
    
    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 2f;
    [SerializeField] private GameObject playerVisual;

    private PlayerController _playerController;
    private CharacterController _characterController;
    private bool _isDead = false;

    public System.Action OnDeathAnimationComplete;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    public void TriggerDeath()
    {
        if (_isDead) return;
        _isDead = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        _playerController.enabled = false;
        _characterController.enabled = false;

        // Switch to QTE action map
        UnityEngine.InputSystem.PlayerInput playerInput =
            GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("QTE");

        WeaponHandler weaponHandler = GetComponent<WeaponHandler>();
        if (weaponHandler != null)
            weaponHandler.enabled = false;

        Animator animator = GetComponentInChildren<Animator>();
        animator?.SetTrigger("Death");

        yield return new WaitForSeconds(deathAnimationDuration);
        OnDeathAnimationComplete?.Invoke();
    }

    public void Revive()
    {
        _isDead = false;
        _playerController.enabled = true;
        _characterController.enabled = true;

        // Switch back to Player action map
        UnityEngine.InputSystem.PlayerInput playerInput =
            GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

        WeaponHandler weaponHandler = GetComponent<WeaponHandler>();
        if (weaponHandler != null)
            weaponHandler.enabled = true;

        HealthSystem health = GetComponent<HealthSystem>();
        health?.ResetHealth();

        Animator animator = GetComponentInChildren<Animator>();
        animator?.SetTrigger("Revive");
    }

    public bool IsDead() => _isDead;
}
