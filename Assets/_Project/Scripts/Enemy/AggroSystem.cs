using System.Collections.Generic;
using UnityEngine;

public class AggroSystem : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float aggroUpdateInterval = 1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float threatDecayRate = 2f;
    [SerializeField] private float minCombatDuration = 5f;

    private List<Transform> _players = new List<Transform>();
    private Dictionary<Transform, float> _threatTable = new Dictionary<Transform, float>();
    private Transform _currentTarget;
    private float _aggroUpdateTimer;
    private float _combatTimer = 0f;
    private bool _isInCombat = false;

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

        DecayThreat();
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

        // Clean dead players from threat table
        List<Transform> deadPlayers = new List<Transform>();
        foreach (var kvp in _threatTable)
        {
            // Check for null before accessing any components
            if (kvp.Key == null)
            {
                deadPlayers.Add(kvp.Key);
                continue;
            }

            HealthSystem health = kvp.Key.GetComponentInParent<HealthSystem>();
            if (health?.IsDead() ?? true)
            {
                deadPlayers.Add(kvp.Key);
            }
        }

        foreach (var dead in deadPlayers)
        {
            _threatTable.Remove(dead);
        }

        // Remove dead/destroyed players from player list
        _players.RemoveAll(p => p == null ||
            p.gameObject == null ||
            !p.gameObject.activeInHierarchy ||
            (p.GetComponentInParent<HealthSystem>()?.IsDead() ?? true));
            if (_players.Count == 0) return;

        // Priority 1 Target highest threat player (combat aggro)
        if (_threatTable.Count > 0)
        {
            UpdateThreatTarget();
            return;
        }

        //Priority 2 Target nearest player (detection aggro)
        _currentTarget = GetNearestPlayer();

    }

    private Transform GetNearestPlayer()
    {
        Transform nearest = null;
        float closestDistance = float.MaxValue;

        foreach (var player in _players)
        {
            // Check for destroyed objects
            if (player == null) continue;

            // Check if dead
            HealthSystem health = player.GetComponentInParent<HealthSystem>();
            if (health == null || health.IsDead()) continue;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = player;
            }
        }

        return nearest;
    }

    public void RegisterThreat(Transform attacker, float threatAmount)
    {
        if (attacker == null) return;
        if (attacker.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        if (!_players.Contains(attacker))
        {
            _players.Add(attacker);
        }

        if (_threatTable.ContainsKey(attacker))
        {
            _threatTable[attacker] += threatAmount;
        }
        else
        {
            _threatTable[attacker] = threatAmount;
        }

        // Start combat timer
        _isInCombat = true;
        _combatTimer = minCombatDuration;

        // Update target to highest threat 
        UpdateThreatTarget();
        Debug.Log($"{gameObject.name} received threat from {attacker.name} | Threat: {_threatTable[attacker]}");
    }

    private void UpdateThreatTarget()
    {
        if (_threatTable.Count == 0) return;

        Transform highestThreatTarget = null;
        float highestThreat = 0f;

        foreach (var kvp in _threatTable)
        {
            // Skip dead players
            HealthSystem health = kvp.Key.GetComponentInParent<HealthSystem>();
            if (health?.IsDead() ?? true) continue;

            if (kvp.Value > highestThreat)
            {
                highestThreat = kvp.Value;
                highestThreatTarget = kvp.Key;
            }
        }

        if (highestThreatTarget != null)
        {
            _currentTarget = highestThreatTarget;
        }
    }

    private void DecayThreat()
    {
        // Count down combat timer
        if (_combatTimer >0f)
        {
            _combatTimer -= Time.deltaTime;
            if (_combatTimer <= 0f)
            {
                _isInCombat = false;
            }
        }

        // Only decay threat when combat timer has expired
        if (_isInCombat) return;

        List<Transform> keys = new List<Transform>(_threatTable.Keys);
        foreach (var key in keys)
        {
            _threatTable[key] -= threatDecayRate * Time.deltaTime;
            if (_threatTable[key] == 0)
            {
                _threatTable.Remove(key);
            }
        }
    }

    public Transform GetCurrentTarget() => _currentTarget;
    public float GetDetectionRange() => detectionRange;
    public bool HasTarget() => _currentTarget != null;
    public bool IsInCombat() => _isInCombat;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);   
    }
}
