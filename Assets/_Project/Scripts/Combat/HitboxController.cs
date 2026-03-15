using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    
    [Header("Hitbox Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private LayerMask hitLayers;

    private bool _isActive = false;
    private List<Collider> _alreadyHit = new List<Collider>();
    private GameObject _owner;

    public void Initialize(GameObject owner)
    {
        _owner = owner;
    }

    public void SetDamage(float damage) => this.damage = damage;
    public void SetKnockback(float knockback) => this.knockbackForce = knockback;

    public void ActivateHitbox()
    {
        _isActive = true;
        _alreadyHit.Clear();
    }

    public void DeactivateHitbox()
    {
        _isActive = false;
        _alreadyHit.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isActive) return;
        if(_alreadyHit.Contains(other)) return;
        if (other.gameObject == _owner) return; 

        HealthSystem health = other.GetComponentInParent<HealthSystem>();
        if (health != null)
        {
            Debug.Log($"Dealing damage to {other.transform.root.name}");
            _alreadyHit.Add(other);
            Vector3 knockbackDir = (other.transform.position - _owner.transform.position).normalized;
            health.TakeDamage(damage, knockbackDir * knockbackForce);
        }
        else
        {
            Debug.Log($"No HealthSystem found on {other.gameObject.name}");
        }
    }

    //Temporary visualizer for hitbox in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isActive ? Color.red : Color.yellow;
        if (TryGetComponent<BoxCollider>(out var box))
        {
            Gizmos.DrawWireCube(transform.position, box.size);
        }
        if (TryGetComponent<SphereCollider>(out var sphere))
        {
            Gizmos.DrawWireSphere(transform.position,sphere.radius);
        }
    }

}
