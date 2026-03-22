using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    // Reference to owning player
    public PlayerController Player { get; private set; }
    public int PlayerIndex { get; private set; }

    private IInteractable _currentInteractable;
    private List<IInteractable> _nearbyInteractables = new List<IInteractable>();
    private float _holdTimer = 0f;
    private bool _isHolding = false;

    private void Awake()
    {
        Player = GetComponentInParent<PlayerController>();
    }

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
    }

    private void Update()
    {
        DetectInteractables();

        if (_isHolding && _currentInteractable != null)
        {
            _holdTimer += Time.deltaTime;
            _currentInteractable.OnInteractHeld(this, _holdTimer);
        }
    }

    private void DetectInteractables()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        // Find closest interactable
        IInteractable closest = null;
        float closestDistance = float.MaxValue;
 
        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.GetComponentInParent<IInteractable>();
            }

            if (interactable != null && interactable.CanInteract(this))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
        }

        // Handle interactable change
        if (closest != _currentInteractable)
        {
            _currentInteractable?.OnInteractableExit(this);
            _currentInteractable = closest;
            _currentInteractable?.OnInteractableEnter(this);
        }
    }

    public void OnInteractPressed()
    {
        if (_currentInteractable == null) return;
        if (!_currentInteractable.CanInteract(this)) return;

        _isHolding = true;
        _holdTimer = 0f;
        _currentInteractable.OnInteract(this);
    }

    public void OnInteractReleased()
    {
        _isHolding = false;
        _holdTimer = 0f;
    }

    public IInteractable GetCurrentInteractable() => _currentInteractable;
    public float GetInteractionRange() => interactionRange;

    private void OizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);    
    }
}
