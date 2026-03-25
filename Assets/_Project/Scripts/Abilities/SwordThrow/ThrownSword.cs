using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class ThrownSword : MonoBehaviour
{
    
    private float _damage;
    private float _speed;
    private float _maxRange;
    private GameObject _owner;
    private Vector3 _startPosition;
    private Vector3 _direction;
    private bool _returning = false;
    private float _distanceTravelled = 0f;
    private List<GameObject> _hitObjects = new List<GameObject>();
    [SerializeField] LayerMask _hitLayers;

    public System.Action OnReturnedToOwner;

    public void Initialize(GameObject owner, float damage, float speed, float maxRange, LayerMask hitLayers)
    {
        _owner = owner;
        _damage = damage;
        _speed = speed;
        _maxRange = maxRange;
        _hitLayers = hitLayers;
        _startPosition = transform.position;
        _direction = owner.transform.forward;
    }

    private void Update()
    {
        if (!_returning)
        {
            MoveForward();
        }
        else
        {
            MoveBack();
        }
    }

    private void MoveForward()
    {
        float moveDistance = _speed * Time.deltaTime;
        transform.position += _direction * moveDistance;
        _distanceTravelled += moveDistance;

        // Rotate sword as it flies
        transform.Rotate(Vector3.right * 720f * Time.deltaTime);

        if (_distanceTravelled >= _maxRange) StartReturning();
    }

    private void MoveBack()
    {
        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 returnDirection = (_owner.transform.position - transform.position).normalized;
        transform.position += returnDirection * _speed * 1.5f * Time.deltaTime;

        // Rotate sword as it returns 
        transform.Rotate(Vector3.right * 720f * Time.deltaTime);

        // Check if returned to owner
        if (Vector3.Distance(transform.position, _owner.transform.position) < 0.5f)
        {
            OnReturnedToOwner?.Invoke();
            Destroy(gameObject);
        }
    }

    public void StartReturning()
    {
        _returning = true;
        _hitObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameObject == _owner) return;
        if (_hitObjects.Contains(other.gameObject)) return;

        HealthSystem health = other.GetComponentInParent<HealthSystem>();
        if (health != null)
        {
            _hitObjects.Add(other.gameObject);
            Vector3 knockback = _direction * 3f;
            health.TakeDamage(_damage, knockback, _owner);
            Debug.Log($"Thrown sword hit: {other.gameObject.name}");
        }

        // Check if hit a wall 
        if (((1 << other.gameObject.layer) & _hitLayers) == 0 && !other.isTrigger) StartReturning();
    }
}
