using UnityEngine;

public enum PickupType { Health, Ammo, Item }

public class Pickup : InteractableBase
{
    
    [Header("Pickup Settings")]
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float value = 25f;
    [SerializeField] private bool autoPickup = true;
    [SerializeField] private float respawnTime = 0f;
    [SerializeField] private GameObject visualObject;

    private void Awake()
    {
        interactPrompt= $"Press E to pick up {pickupType}";
        requiresBothPlayers = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoPickup) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null) ApplyPickup(player);
    }

    protected override void Execute(InteractionDetector detector)
    {
        ApplyPickup(detector.Player);
    }

    private void ApplyPickup(PlayerController player)
    {
        if (!isInteractable) return;

        switch (pickupType)
        {
            case PickupType.Health:
                HealthSystem health = player.GetComponent<HealthSystem>();
                health?.Heal(value);
                break;
            case PickupType.Ammo:
                // TODO: Wire to inventory system
                Debug.Log($"{player.gameObject.name} picked upp ammo: +{value}");
                break;
            case PickupType.Item:
                // TODO : Wire to inventory system
                Debug.Log($"{player.gameObject.name} picked up item");
                break;
        }

        if (respawnTime > 0f)
        {
            isInteractable = false;
            if (visualObject != null) visualObject.SetActive(false);
            StartCoroutine(Respawn());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        isInteractable = true;
        if (visualObject != null) visualObject.SetActive(true);
    }
}
