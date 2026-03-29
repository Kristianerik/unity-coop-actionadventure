using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    
    [Header("Checkpoint Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool activateOnTrigger = true;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private GameObject inactiveVisual;

    private bool _isActivated = false;

    private void Awake()
    {
        // If no spawn point assigned use this object's position
        if (spawnPoint == null) spawnPoint = transform;

        UpdateVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activateOnTrigger) return;
        if (_isActivated) return;

        // Only activate when player walks through
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
        {
            Activate();
        }
    }

    public void Activate()
    {
        if (_isActivated) return;
        _isActivated = true;
        CheckpointManager.Instance?.SetCheckpoint(this);
        UpdateVisuals();
        Debug.Log($"Checkpoint activated: {gameObject.name}");
    }

    private void UpdateVisuals()
    {
        if (activeVisual != null) activeVisual.SetActive(_isActivated);
        if (inactiveVisual != null) inactiveVisual.SetActive(!_isActivated);
    }

    public Transform GetSpawnPoint() => spawnPoint;
    public bool IsActivated() => _isActivated;
}
