using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<Vector3> OnDamageTaken; // knockback direction
    public event Action OnDeath;

    private bool _isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector3 knockback = default)
    {
        if (_isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(knockback);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if(_isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public float GetHealthPercent () => currentHealth / maxHealth;
    public bool isDead() => _isDead;

    private void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();
    }

}
