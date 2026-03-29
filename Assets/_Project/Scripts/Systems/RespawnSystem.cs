using System.Collections;
using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    
    [Header("Respawn Settings")]
    [SerializeField] private float baseRespawnTime = 10f;
    [SerializeField] private Transform respawnPoint;

    private float _respawnTimer;
    private bool _isRespawning = false;
    private PlayerDeathState _playerDeathState;
    private HealthSystem _healthSystem;
    private QTESystem _qteSystem;

    public System.Action<float> OnRespawnTimerUpdate;
    public System.Action OnRespawned;

    private void Awake()
    {
        _playerDeathState = GetComponent<PlayerDeathState>();
        _healthSystem = GetComponent<HealthSystem>();
        _qteSystem = GetComponent<QTESystem>();
    }

    private void Start()
    {
        _healthSystem.OnDeath += TriggerDeath;
        _qteSystem.OnQTESuccess += ReduceRespawnTimer;
    }

    private void TriggerDeath()
    {
        if (_isRespawning) return;
        _playerDeathState?.TriggerDeath();
        _playerDeathState.OnDeathAnimationComplete += StartRespawnCountdown;
    }

    private void StartRespawnCountdown()
    {
        _isRespawning = true;
        _respawnTimer = baseRespawnTime;
        _qteSystem.StartQTE();
        StartCoroutine(RespawnCountdown());
    }

    private IEnumerator RespawnCountdown()
    {
        while (_respawnTimer > 0f)
        {
            _respawnTimer -= Time.deltaTime;
            OnRespawnTimerUpdate?.Invoke(_respawnTimer);
            Debug.Log($"Respawning in: {_respawnTimer:0.0}s");
            yield return null;
        }

        Respawn();
    }

    private void ReduceRespawnTimer(float reduction)
    {
        _respawnTimer = Mathf.Max(0f, _respawnTimer - reduction);
        Debug.Log($"Respawn timer reduced! New time: {_respawnTimer:0.0}s");
    }

    private void Respawn()
    {
        _isRespawning = false;
        _qteSystem?.StopQTE();

        // Reset position
        if (respawnPoint != null) transform.position = respawnPoint.position;

        // Restore health
        _healthSystem.Heal(_healthSystem.GetHealthPercent() > 0f ? 0f : 100f);
        _healthSystem?.Heal(100f);

        // Revive player
        _playerDeathState?.Revive();

        OnRespawned?.Invoke();
        Debug.Log($"{gameObject.name} respawned!");
    }

    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
    }

    public void ForceRespawn()
    {
        StopAllCoroutines();
        _isRespawning = false;
        _qteSystem?.StopQTE();
        Respawn();
    }

    public bool IsRespawning() => _isRespawning;
    public float GetRespawnTimer() => _respawnTimer;
}
