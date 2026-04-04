using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    
    public static CheckpointManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private Transform defaultSpawnPoint1;
    [SerializeField] private Transform defaultSpawnPoint2;


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
        Debug.Log($"CheckpointManager found {players.Length} players");
        foreach (var player in players)
        {
            Debug.Log($"Found player: {player.name}");
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
        return defaultSpawnPoint1;
    }

    public void RestartFromCheckpoint()
    {
        _snapshot.RestoreSnapshot();

        // Spawn players side by side with offset
        for (int i = 0; i < _respawnSystems.Count; i++)
        {
            Transform spawnPoint = GetSpawnPoint(i);
            
            _respawnSystems[i].SetRespawnPoint(spawnPoint);
            _respawnSystems[i].ForceRespawn();
        }

        // Refresh all aggro systems
        AggroSystem[] aggroSystems = FindObjectsByType<AggroSystem>(FindObjectsSortMode.None);
        foreach (var aggro in aggroSystems) aggro.ForceRefresh();
    }

    private Transform GetSpawnPoint(int playerIndex)
    {
        if (_currentCheckpoint != null)
            return _currentCheckpoint.GetSpawnPoint(playerIndex);

        // Fall back to default spawn points
        if (playerIndex == 1 && defaultSpawnPoint2 != null)
            return defaultSpawnPoint2;

        return defaultSpawnPoint1;
    }
}
