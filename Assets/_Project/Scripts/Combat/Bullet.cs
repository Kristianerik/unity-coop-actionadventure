using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private GameObject _owner;
    private float _lifetime = 5f;

    public void Initialize(GameObject owner, float damage, float speed)
    {
        _owner = owner;
        _damage = damage;
        _speed = speed;
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _owner) { return; }

        if (other.TryGetComponent<HealthSystem>(out var health))
        {
            Vector3 knockback = transform.forward * 3f;
            health.TakeDamage(_damage, knockback);
        }
        
        Destroy(gameObject);
    }
}
