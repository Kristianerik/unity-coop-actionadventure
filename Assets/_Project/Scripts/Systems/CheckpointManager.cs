using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    
    public static CheckpointManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private Transform defaultSpawnPoint;

    private Checkpoint _currentCheckpoint;
    private List<RespawnSystem> _respawnSystems = new List<RespawnSystem>();
    private GameStateSnapshot _snapshot = new GameStateSnapshot();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            RespawnSystem respawn = player.GetComponent<RespawnSystem>();
            if (respawn != null) _respawnSystems.Add(respawn);
        }

        RegisterAllResettables();
        _snapshot.TakeSnapshot();
    }

    private void RegisterAllResettables()
    {
        MonoBehaviour[] allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj is IResettable resettable) _snapshot.RegisterResettable(resettable);
        }

        Debug.Log($"Registered {allObjects.Length} resettable objects");
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        _currentCheckpoint = checkpoint;
        
        _snapshot.TakeSnapshot();

        // Update all respawn systems with new chepoint position
        foreach (var respawn in _respawnSystems) respawn.SetRespawnPoint(_currentCheckpoint.GetSpawnPoint());

        Debug.Log($"Checkpoint set: {checkpoint.gameObject.name}");
    }

    public Transform GetCurrentSpawnPoint()
    {
        if (_currentCheckpoint != null) return _currentCheckpoint.GetSpawnPoint();
        return defaultSpawnPoint;
    }

    public void RestartFromCheckpoint()
    {
        _snapshot.RestoreSnapshot();

        // Respawn all players at  checkpoint
        foreach (var respawn in _respawnSystems)
        {
            Transform spawnPoint = _currentCheckpoint != null ? _currentCheckpoint.GetSpawnPoint() : defaultSpawnPoint;
            respawn.SetRespawnPoint(spawnPoint);
            respawn.ForceRespawn();
        }

        Debug.Log($"Restarting from checkpoint: {_currentCheckpoint.gameObject.name}");
    }

    private void RestartFromDefault()
    {
        foreach (var respawn in _respawnSystems)
        {
            if (defaultSpawnPoint != null) respawn.SetRespawnPoint(defaultSpawnPoint);
            respawn.ForceRespawn();
        }
    }
}
