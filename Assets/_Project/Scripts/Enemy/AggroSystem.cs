using System.Collections.Generic;
using UnityEngine;

public class AggroSystem : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float aggroUpdateInterval = 1f;
    [SerializeField] private LayerMask playerLayer;

    private List<Transform> _players = new List<Transform>();
    private Transform _currentTarget;
    private float _aggroUpdateTimer;

    // Static registry of all aggro systems for split aggro
    private static List<AggroSystem> _allAggroSystems = new List<AggroSystem>();

    private void OnEnable() => _allAggroSystems.Add(this);
    private void OnDisable() => _allAggroSystems.Remove(this);

    private void Start()
    {
        RefreshPlayerList();
    }

    private void Update()
    {
        _aggroUpdateTimer += Time.deltaTime;
        if (_aggroUpdateTimer >= aggroUpdateInterval)
        {
            _aggroUpdateTimer = 0f;
            UpdateTarget();
        }
    }

    private void RefreshPlayerList()
    {
        _players.Clear();
        Collider[] playerColliders = Physics.OverlapSphere (transform.position, detectionRange * 2f, playerLayer);

        foreach (var col in playerColliders) {
            Transform root = col.transform.root;
            if (!_players.Contains(root))
            {
                _players.Add(root);
            }
        }
    }

    private void UpdateTarget()
    {
        if (_players.Count == 0)
        {
            RefreshPlayerList();
            return;
        }

        _players.RemoveAll(p => p == null || (p.GetComponentInParent<HealthSystem>()?.IsDead() ?? true));

        if (_players.Count == 0) return;

        _currentTarget = GetLeastTargetedPlayer();
    }

    private Transform GetLeastTargetedPlayer()
    {
        // Count how many enemies are targeting each player
        Dictionary<Transform, int> targetCount = new Dictionary<Transform, int>();

        foreach (var player in _players)
        {
            HealthSystem health = player.GetComponentInParent<HealthSystem>();
            if (health?.IsDead() ?? true) continue;
            targetCount[player] = 0;
        }

        foreach (var aggro in _allAggroSystems)
        {
            if (aggro == this) continue;
            if (aggro._currentTarget != null && targetCount.ContainsKey(aggro._currentTarget))
            {
                targetCount[aggro._currentTarget]++;
            }
        }

        // Find the player with the least targets
        // If tied. Pick closest one
        Transform bestTarget = null;
        int lowestCount = int.MaxValue;
        float closestDistance = float.MaxValue;

        foreach (var kvp in targetCount)
        {
            float distance = Vector3.Distance(transform.position, kvp.Key.position);

            if (kvp.Value < lowestCount || (kvp.Value == lowestCount && distance < closestDistance))
            {
                lowestCount = kvp.Value;
                closestDistance = distance;
                bestTarget = kvp.Key;
            }
        }

        return bestTarget;
    }

    public Transform GetCurrentTarget() => _currentTarget;
    public float GetDetectionRange() => detectionRange;
    public bool HasTarget() => _currentTarget != null;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);   
    }
}
