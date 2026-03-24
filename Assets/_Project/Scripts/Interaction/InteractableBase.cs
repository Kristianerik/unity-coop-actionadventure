using Unity.VisualScripting;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    
    [Header("Interaction Settings")]
    [SerializeField] protected string interactPrompt = "Press E to interact";
    [SerializeField] protected bool requiresBothPlayers  = false;
    [SerializeField] protected bool isInteractable = true;

    protected int _playersInRange = 0;
    protected int _playersInteracting = 0;


    public virtual void OnInteractableEnter(InteractionDetector detector)
    {
        _playersInRange++;
    }

    public virtual void OnInteractableExit(InteractionDetector detector)
    {
        _playersInRange--;
    }

    public virtual void OnInteract(InteractionDetector detector)
    {
        _playersInteracting++;

        if (requiresBothPlayers && _playersInteracting < 2)
        {
            Debug.Log($"Waiting for secont player... {_playersInteracting}/2");
            return;
        }

        Execute(detector);
        _playersInteracting = 0;
    }

    public virtual void OnInteractHeld(InteractionDetector detector, float holdTime) { }

    public virtual bool CanInteract(InteractionDetector detector)
    {
        return isInteractable;
    }

    public virtual string GetInteractPrompt()
    {
        if (requiresBothPlayers) return $"{interactPrompt} (Requires both players)";

        return interactPrompt;
    }
    
    protected abstract void Execute(InteractionDetector detector);

    public void SetInteractable(bool value) => isInteractable = value;
    
}
