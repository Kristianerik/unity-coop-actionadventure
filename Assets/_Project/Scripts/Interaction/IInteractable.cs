using UnityEditor;
using UnityEngine;

public interface IInteractable 
{
    
    // Called when player enters interaction range
    void OnInteractableEnter(InteractionDetector detector);

    // Called when player exits interaction range
    void OnInteractableExit(InteractionDetector detector);

    // Called when player presses interaction button
    void OnInteract(InteractionDetector detector);

    // Called when every frame while player holds interact button
    void OnInteractHeld(InteractionDetector detector, float holdTime);

    // Whether this interactable is currently usable
    bool CanInteract(InteractionDetector detector);

    // Display name shown in UI prompt
    string GetInteractPrompt();
}
