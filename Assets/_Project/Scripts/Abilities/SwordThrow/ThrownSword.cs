using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ThrownSword : MonoBehaviour
{
    
    private float _damage;
    private float _speed;
    private float _maxRange;
    private GameObject _owner;
    private Vector3 _startPosition;
    private Vector3 _direction;
    private bool _returning = false;
    private bool _hasReturned = false;
    private float _distanceTravelled = 0f;
    private float _returnHeight;
    private List<GameObject> _hitObjects = new List<GameObject>();
    [SerializeField] LayerMask _hitLayers;

    public System.Action OnReturnedToOwner;

    public void Initialize(GameObject owner, float damage, float speed, float maxRange, LayerMask hitLayers, float returnHeight = 1.2f)
    {
        _owner = owner;
        _damage = damage;
        _speed = speed;
        _maxRange = maxRange;
        _hitLayers = hitLayers;
        _startPosition = transform.position;
        _direction = owner.transform.forward;
        _returnHeight = returnHeight;
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

        Vector3 returnTarget = _owner.transform.position + Vector3.up * _returnHeight;

        Vector3 returnDirection = (returnTarget - transform.position).normalized;
        transform.position += returnDirection * _speed * 1.5f * Time.deltaTime;
        transform.Rotate(Vector3.right * 720f * Time.deltaTime);

        // Recalculate distance AFTER moving
        float distanceAfterMove = Vector3.Distance(transform.position, returnTarget);

        // Check if returned to owner
        if (!_hasReturned && distanceAfterMove < 0.5f)
        {
            _hasReturned = true;
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
