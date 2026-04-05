using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    
    [Header("Checkpoint Settings")]
    [SerializeField] private bool activateOnTrigger = true;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private GameObject inactiveVisual;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    private bool _isActivated = false;

    private void Awake()
    {
        // If no spawn point assigned use this object's position
        if (spawnPoint1 == null) spawnPoint1 = transform;

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

        GameManager.Instance?.SetCheckpoint(
            gameObject.name, 
            spawnPoint1.position, 
            spawnPoint2 != null ? spawnPoint2.position : spawnPoint1.position + Vector3.right * 1.5f
        );
        
        UpdateVisuals();
        Debug.Log($"Checkpoint activated: {gameObject.name}");
    }

    private void UpdateVisuals()
    {
        if (activeVisual != null) activeVisual.SetActive(_isActivated);
        if (inactiveVisual != null) inactiveVisual.SetActive(!_isActivated);
    }

    public Transform GetSpawnPoint(int playerIndex = 0) 
    {
        if (playerIndex == 1 && spawnPoint2 != null) return spawnPoint2;
        return spawnPoint1;
    }
    public bool IsActivated() => _isActivated;
}
