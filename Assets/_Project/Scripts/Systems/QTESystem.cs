using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class QTESystem : MonoBehaviour
{
    
    [Header("QTE Settings")]
    [SerializeField] private float timeReductionPerSuccess = 0.5f;
    [SerializeField] private float qteWindowDuration = 1f;
    [SerializeField] private int maxQTEAttempts = 5;

    private bool _isActive = false;
    private bool _waitingForInput = false;
    private int _attemptsRemaining;
    private float _qteTimer = 0f;

    public System.Action<float> OnQTESuccess;
    public System.Action OnQTEFailed;
    public System.Action OnQTEComplete;

    public void StartQTE()
    {
        _isActive = true;
        _attemptsRemaining = maxQTEAttempts;
        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        while (_attemptsRemaining > 0 && _isActive)
        {
            // Signal player to press button
            _waitingForInput = true;
            _qteTimer = 0f;
            Debug.Log($"QTE! Press button! Attempts remaining: {_attemptsRemaining}");

            while (_qteTimer < qteWindowDuration && _waitingForInput)
            {
                _qteTimer += Time.deltaTime;
                yield return null;
            }

            if (_waitingForInput)
            {
                // Missed input
                Debug.Log("QTE Failed!");
                OnQTEFailed?.Invoke();
            }

            _attemptsRemaining--;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnQTEInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_waitingForInput) return;

        _waitingForInput = false;
        float timeReduction = timeReductionPerSuccess * (1f - (_qteTimer / qteWindowDuration));
        Debug.Log($"QTE Success! Time reduction: {timeReduction} seconds");
        OnQTESuccess?.Invoke(timeReduction);
    }

    private bool IsActive() => _isActive;
    public void StopQTE() => _isActive = false;
}
