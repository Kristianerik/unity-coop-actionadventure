using System;
using NUnit.Framework;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    
    [Header("Weak Point Settings")]
    [SerializeField] private float damageMultiplier = 2.5f;
    [SerializeField] private bool isActive = false;
    [SerializeField] private LayerMask attackLayers;

    public event Action<float> OnWeakPointHit;
    public event Action OnWeakPointDestroyed;

    private HealthSystem _bossHealth;
    private bool _isDestroyed = false;

    public void Initialize(HealthSystem bossHealth)
    {
        _bossHealth = bossHealth;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        // Visual feedback - change color when active
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = active ? Color.red : Color.grey;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"WeakPoint trigger entered by: {other.gameObject.name} | layer: {other.gameObject.layer} | isActive: {isActive}");
        if (!isActive || _isDestroyed) return;

        // Check if hit by player attack
        if (((1 << other.gameObject.layer) & attackLayers) != 0)
        {
            float damage = 10f * damageMultiplier;
            _bossHealth?.TakeDamage(damage, Vector3.zero);
            OnWeakPointHit?.Invoke(damage);
            Debug.Log($"Weak point hit! Damage: {damage}");

            // Destroy weak point after being hit
            _isDestroyed = true;
            SetActive(false);
            OnWeakPointDestroyed?.Invoke();
        }
    }

    public bool IsActive() => isActive;
    public bool IsDestroyed() => _isDestroyed;
}
