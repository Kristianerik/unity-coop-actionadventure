using UnityEngine;
using UnityEngine.Events;

public class Lever : InteractableBase
{
    
    [Header("Lever Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private float holdTimeRequired = 0f;
    [SerializeField] private Transform leverHandle;
    [SerializeField] private float activatedAngle = 45f;

    [Header("Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    private void Awake()
    {
        interactPrompt = holdTimeRequired > 0f ? $"Hold E for {holdTimeRequired}s to activate" : "Press E to activate lever";
    }

    public override void OnInteractHeld(InteractionDetector detector, float holdTime)
    {
        if (holdTimeRequired <= 0f) return;

        if(holdTime >= holdTimeRequired) Execute(detector);
    }

    public override void OnInteract(InteractionDetector detector)
    {
        if (holdTimeRequired > 0f) return;
        base.OnInteract(detector);   
    }

    protected override void Execute(InteractionDetector detector)
    {
        isActivated = !isActivated;

        // Rotate lever handle
        if (leverHandle != null)
        {
            leverHandle.localRotation = Quaternion.Euler(isActivated ? activatedAngle : 0f, 0f, 0f);
        }

        if (isActivated)
        {
            OnActivated?.Invoke();
            Debug.Log($"{gameObject.name} activated!");
        }
        else
        {
            OnDeactivated?.Invoke();
            Debug.Log($"{gameObject.name} deactivated!");
        }

        interactPrompt = isActivated ? "Press E to deactivate" : "Press E to activate lever";
    }
}
