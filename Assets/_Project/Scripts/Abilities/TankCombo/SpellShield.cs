using UnityEngine;

public class SpellShield : SpellBase
{
    
    [Header("Shield Settings")]
    [SerializeField] private float shieldHealth = 50f;
    [SerializeField] private float shieldRadius = 1.5f;

    private float _currentShieldHealth;
    private bool _isActive = false;

    public override void Cast(Vector3 position, Vector3 direction)
    {
        _currentShieldHealth = shieldHealth;
        _isActive = true;
        transform.localScale = Vector3.one * shieldRadius * 2f;

        // Follow caster 
        transform.SetParent(_caster.transform);
        transform.localPosition = Vector3.zero;

        Destroy(gameObject, duration);
        Debug.Log($"Shield activated! HP: {_currentShieldHealth}"); 
    }

    private void OTriggerEnter(Collider other)
    {
        if (!_isActive) return;
        if (other.gameObject == _caster) return;

        // Absorb damage from projectile
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            _currentShieldHealth -= 10f;
            Destroy(other.gameObject);
            Debug.Log($"Shield absorbed hit! HP: {_currentShieldHealth}");

            if (_currentShieldHealth <= 0f)
            {
                _isActive = false;
                Destroy(gameObject);
            }
        }
    }
}
