using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class PunchingBag : InteractableBase
{
    
    [SerializeField] private PunchingBagMinigame minigame;
    private PlayerController _currentPlayer;
    private bool _minigameActive = false;

    private void Awake()
    {
        interactPrompt = "Press E to use punching bag";
    }

    protected override void Execute(InteractionDetector detector)
    {
        if (_minigameActive) return;

        _currentPlayer = detector.Player;
        _minigameActive = true;

        // Switch player to minigame input
        SwitchToMinigameInput();

        minigame.StartMinigame();
        minigame.OnMinigameComplete += HandleMinigameComplete;

        interactPrompt = "Press E to stop"; 
    }

    public override void OnInteract(InteractionDetector detector)
    {
        if (_minigameActive)
        {
            StopMinigame();
            return;
        }
        base.OnInteract(detector);
    }

    public void SwitchToMinigameInput()
    {
        WeaponHandler weaponHandler = _currentPlayer.GetComponent<WeaponHandler>();
        if (weaponHandler != null) weaponHandler.enabled = false;

        // Switch to minigame action map
        UnityEngine.InputSystem.PlayerInput playerInput = _currentPlayer.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null && playerInput.actions != null) playerInput.SwitchCurrentActionMap("Minigame");

        _currentPlayer.SetMinigame(minigame);
    }

    private void RestorePlayerInput()
    {
        WeaponHandler weaponHandler = _currentPlayer.GetComponent<WeaponHandler>();
        if (weaponHandler != null) weaponHandler.enabled = true;

        // Switch back to Player action map
        UnityEngine.InputSystem.PlayerInput playerInput = _currentPlayer.GetComponent<UnityEngine.InputSystem.PlayerInput>();

        if (playerInput != null && playerInput.actions != null) playerInput.SwitchCurrentActionMap("Player");

        _currentPlayer.SetMinigame(null);
    }

    private void HandleMinigameComplete(int score)
    {
        _minigameActive = false;
        RestorePlayerInput();
        minigame.OnMinigameComplete -= HandleMinigameComplete;
        interactPrompt = "Press E to use punching bag";
    }

    private void StopMinigame()
    {
        _minigameActive = false;
        minigame.StopMinigame();
        RestorePlayerInput();
        minigame.OnMinigameComplete -= HandleMinigameComplete;
        interactPrompt = "Press E to use punching bag";
    }
}
