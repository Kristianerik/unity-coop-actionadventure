using System.Collections.Generic;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private float gameOverDelay = 2f;
    private List<HealthSystem> _playerHealthSystems = new List<HealthSystem>();
    private bool _isGameOver = false;

    public System.Action OnGameOver;

    private void Start()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in playerObjects)
        {
            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                _playerHealthSystems.Add(health);
                health.OnDeath += CheckGameOver;
            }
        }
    }

    private void CheckGameOver()
    {
        if (_isGameOver) return;
        StartCoroutine(DelayedGameOverCheck());
    }

    private System.Collections.IEnumerator DelayedGameOverCheck()
    {
        yield return new WaitForSeconds(0.5f);

        bool anyPlayerAlive = false;
        foreach (var health in _playerHealthSystems)
        {
            if (!health.IsDead())
            {
                anyPlayerAlive = true;
                break;
            }
        }

        if (!anyPlayerAlive) StartCoroutine(TriggerGameOver());
    }

    private System.Collections.IEnumerator TriggerGameOver()
    {
        _isGameOver = true;
        Debug.Log("Game Over! All players are dead.");
        OnGameOver?.Invoke();

        yield return new WaitForSeconds(gameOverDelay);

        CheckpointManager.Instance?.RestartFromCheckpoint();
        _isGameOver = false;
    }

    public bool IsGameOver() => _isGameOver;
}
