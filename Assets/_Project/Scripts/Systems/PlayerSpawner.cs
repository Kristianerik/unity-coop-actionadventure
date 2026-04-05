using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class PlayerSpawner : MonoBehaviour
{
    
    [Header("Player Prefabs")]
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject gunnerPrefab;

    [Header("Default Spawn Point")]
    [SerializeField] private Transform defaultSpawn1;
    [SerializeField] private Transform defaultSpawn2;

    private void Start()
    {
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        Vector3 spawn1 = defaultSpawn1.position;
        Vector3 spawn2 = defaultSpawn2 != null ? defaultSpawn2.position : defaultSpawn1.position + Vector3.right * 1.5f;

        // Use checkpoint pos if available
        if (GameManager.Instance != null && GameManager.Instance.HasCheckpoint())
        {
            spawn1 = GameManager.Instance.LastCheckpointPosition1;
            spawn2 = GameManager.Instance.LastCheckpointPosition2;
            Debug.Log($"Spawning at checkpoint: {GameManager.Instance.LastCheckpointID}");
        }
        else
        {
            Debug.Log("No checkpoint found, spawning at default positions.");
        }

        Instantiate(swordsmanPrefab, spawn1, Quaternion.identity);
        Instantiate(gunnerPrefab, spawn2, Quaternion.identity);
    }
}
