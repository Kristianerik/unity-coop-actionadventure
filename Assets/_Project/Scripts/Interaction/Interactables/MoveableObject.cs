using UnityEngine;
using UnityEngine.Timeline;

public class MoveableObject : InteractableBase
{
    
    [Header("Moveable Settings")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float maxMoveSpeed = 3f;
    [SerializeField] private bool requiresBothPlayersToPush = false;

    private Rigidbody _rigidbody;
    private int _playersPushing = 0;
    Vector3 _pushDirection;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        requiresBothPlayers = requiresBothPlayersToPush;
        interactPrompt = requiresBothPlayers ? "Hold E with both pleyers to push" : "Hold E to push";
    }

    public override void OnInteractHeld(InteractionDetector detector, float holdTime)
    {
        _playersPushing++;

        if (requiresBothPlayers && _playersPushing < 2) return;

        // Push in direction player is facing\
        _pushDirection = detector.Player.transform.forward;
        _pushDirection.y = 0f;

        if (_rigidbody != null)
        {
            _rigidbody.AddForce(_pushDirection * pushForce, ForceMode.Force);
            _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, maxMoveSpeed);
        }
    }

    public override void OnInteractableExit(InteractionDetector detector)
    {
        base.OnInteractableExit(detector);
        _playersPushing = 0;

        // Stop object when player releases
        if (_rigidbody != null) _rigidbody.linearVelocity = Vector3.zero;
    }

    protected override void Execute(InteractionDetector detector) { }
}
