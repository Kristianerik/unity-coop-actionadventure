using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using Debug = UnityEngine.Debug;

public class PunchingBag : InteractableBase
{
    
    private enum MinigameState { Inactive, Playing, ShowingResults }
    private MinigameState _state = MinigameState.Inactive;  

    [SerializeField] private PunchingBagMinigame minigame;
    private PlayerController _currentPlayer;

    private void Awake()
    {
        interactPrompt = "Press E to use punching bag";
    }

    protected override void Execute(InteractionDetector detector)
    {
        if (_state != MinigameState.Inactive) return;

        _currentPlayer = detector.Player;
        _state = MinigameState.Playing;

        SwitchToMinigameInput();
        minigame.FindUIForPlayer(_currentPlayer.gameObject);
        minigame.StartMinigame();
        minigame.OnMinigameComplete += HandleMinigameComplete;
        interactPrompt = "Press E to stop";
    }

    public override void OnInteract(InteractionDetector detector)
    {
        if (_state == MinigameState.Playing)
        {
            ForceStop();
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
        _state = MinigameState.ShowingResults;
        minigame.OnMinigameComplete -= HandleMinigameComplete;
    }
    public void ForceStop()
    {
        if (_state == MinigameState.Inactive) return;
        _state = MinigameState.Inactive;
        minigame.StopMinigame();
        RestorePlayerInput();
        interactPrompt = "Press E to use punching bag";
    }
}
