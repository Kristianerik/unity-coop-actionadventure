using UnityEngine;

public class SpellProjectile : SpellBase
{
    
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private float scale = 1f;

    private Vector3 _direction;
    private bool _initialized = false;

    public override void Cast(Vector3 position, Vector3 direction)
    {
        _direction = direction;
        _initialized = true;
        transform.localScale = Vector3.one * scale;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!_initialized) return;
        transform.position += _direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.forward * 360f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _caster) return;

        HealthSystem health = other.GetComponentInParent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage, _direction * 5f, _caster);
            Destroy(gameObject);
        }
    }
}
