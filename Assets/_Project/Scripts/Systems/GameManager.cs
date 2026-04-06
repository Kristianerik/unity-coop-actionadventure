using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    public string LastCheckpointID { get; private set; }
    public Vector3 LastCheckpointPosition1 { get; private set; }
    public Vector3 LastCheckpointPosition2 { get; private set; }
    private bool _hasCheckpoint = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(string id, Vector3 pos1, Vector3 pos2)
    {
        LastCheckpointID = id;
        LastCheckpointPosition1 = pos1;
        LastCheckpointPosition2 = pos2;
        _hasCheckpoint = true;
        Debug.Log($"Checkpoint set: {id} at positions {pos1} and {pos2}");
    }

    public void ResetCheckpoint()
    {
        LastCheckpointID = string.Empty;
        LastCheckpointPosition1 = Vector3.zero;
        LastCheckpointPosition2 = Vector3.zero;
        _hasCheckpoint = false;
    }

    public void RestartFromCheckpoint()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool HasCheckpoint() => _hasCheckpoint; 
}