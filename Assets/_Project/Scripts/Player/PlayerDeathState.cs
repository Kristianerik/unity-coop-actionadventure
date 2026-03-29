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
        // Disable player control
        _playerController.enabled = false;
        _characterController.enabled = false;

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

        Animator animator = GetComponentInChildren<Animator>();
        animator?.SetTrigger("Revive");
    }

    public bool IsDead() => _isDead;
}
