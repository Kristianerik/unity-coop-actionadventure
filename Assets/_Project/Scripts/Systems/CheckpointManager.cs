using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    
    public static CheckpointManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private Transform defaultSpawnPoint;

    private Checkpoint _currentCheckpoint;
    private List<RespawnSystem> _respawnSystems = new List<RespawnSystem>();

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
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        _currentCheckpoint = checkpoint;
        Debug.Log($"Checkpoint set: {checkpoint.gameObject.name}");

        // Update all respawn systems with new chepoint position
        foreach (var respawn in _respawnSystems) respawn.SetRespawnPoint(_currentCheckpoint.GetSpawnPoint());
    }

    public Transform GetCurrentSpawnPoint()
    {
        if (_currentCheckpoint != null) return _currentCheckpoint.GetSpawnPoint();
        return defaultSpawnPoint;
    }

    public void RestartFromCheckpoint()
    {
        if(_currentCheckpoint == null)
        {
            Debug.Log("no checpoint set - restarting from default spawn");
            RestartFromDefault();
            return;
        }

        // Respawn all players at  checkpoint
        foreach (var respawn in _respawnSystems)
        {
            respawn.SetRespawnPoint(_currentCheckpoint.GetSpawnPoint());
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
