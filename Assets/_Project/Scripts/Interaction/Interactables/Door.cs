using System.Collections;
using UnityEngine;

public class Door : InteractableBase, IResettable
{
    
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 3f;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private bool _isMoving = false;
    private AudioSource _audioSource;
    private bool _savedIsOpen;
    private Quaternion _savedRotation;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _closedRotation = transform.rotation;
        _openRotation = transform.rotation * Quaternion.Euler(0f, openAngle, 0f);
        interactPrompt = "Press E to open door";
    }

    protected override void Execute(InteractionDetector detector)
    {
        if (_isMoving) return;
        ToggleDoor();
    }

    public void SaveState()
    {
        _savedIsOpen = isOpen;
        _savedRotation = transform.rotation;
    }

    public void RestoreState()
    {
        StopAllCoroutines();
        isOpen = _savedIsOpen;
        transform.rotation = _savedRotation;
        _isMoving = false;
        interactPrompt = isOpen ? "Press E to close door" : "Press E to open door";
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(MoveDoor(isOpen ? _openRotation : _closedRotation));

        if (isOpen && autoClose) StartCoroutine(AutoClose());

        interactPrompt = isOpen ? "Press E to close door" : "Press E to open door";
    }

    private IEnumerator MoveDoor(Quaternion targetRotation)
    {
        _isMoving = true;

        if (_audioSource != null)
        {
            _audioSource.clip = isOpen ? openSound : closeSound;
            _audioSource.Play();
        }

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;
        _isMoving = false;
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (isOpen) ToggleDoor();
    }
}
