using System;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class PunchingBagMinigame : MinigameBase
{
    
    
    [Header("Minigame Settings")]
    [SerializeField] private float initialDialSpeed = 90f;
    [SerializeField] private float dialSpeedIncrement = 5f;
    [SerializeField] private float maxDialSpeed = 270f;
    [SerializeField] private float initialWindowSize = 60f;
    [SerializeField] private float windowShrinkOnHit = 3f;
    [SerializeField] private float windowGrowOnMiss = 10f;
    [SerializeField] private float minWindowSize = 10f;
    [SerializeField] private float maxWindowSize = 90f;
    [SerializeField] private float hitPower = 15f;
    [SerializeField] private float missPenaltyPercent = 0.05f;
    [SerializeField] private float gameDuration = 45f;

    [Header("UI References")]
    [SerializeField] private GameObject minigameUI;
    [SerializeField] private RectTransform dialNeedle;
    [SerializeField] private RectTransform hitWindow;
    [SerializeField] private Image powerMeterFill;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hitsText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private float powerDisplayCap = 500f;

    // State
    private bool _isActive = false;
    private float _dialAngle = 0f;
    private float _currentDialSpeed;
    private float _currentWindowSize;
    private float _currentWindowPosition = 270f;
    private float _powerMeter = 0f;
    private float _timeRemaining;
    private int _totalHits = 0;
    private int _totalMisses = 0;

    private void Start()
    {
        if (minigameUI != null) minigameUI.SetActive(false);
    }

    public override void StartMinigame()
    {
        _isActive = true;
        _dialAngle = 0f;
        _currentDialSpeed = initialDialSpeed;
        _currentWindowSize = initialWindowSize;
        _currentWindowPosition = 270f;
        _powerMeter = 0f;
        _timeRemaining = gameDuration;
        _totalHits = 0;
        _totalMisses = 0;

        if (minigameUI != null) minigameUI.SetActive(true);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        UpdateUI();
        StartMinigameCoroutine(RunMinigame());
    }

    private IEnumerator RunMinigame()
    {
        while (_timeRemaining > 0 && _isActive)
        {
            _timeRemaining -= Time.deltaTime;

            // Rotate dial
            _dialAngle += _currentDialSpeed * Time.deltaTime;
            if (_dialAngle >= 360f) _dialAngle -= 360f;

            UpdateDialVisual();
            UpdateUI();
            yield return null;
        }

        EndMinigame();
    }

    public override void OnHit()
    {
        if (!_isActive) return;

        float windowStart = GetWindowStartAngle();
        float windowEnd = windowStart + _currentWindowSize;
        bool inWindow = IsAngleInWindow(_dialAngle, windowStart, windowEnd);

        if (inWindow)
        {
            // Hit in window - build power
            _powerMeter += hitPower;
            _totalHits++;

            // Shrink window and speed up needle
            _currentWindowSize = Mathf.Max(minWindowSize, _currentWindowSize - windowShrinkOnHit);
            _currentDialSpeed = Mathf.Min(maxDialSpeed, _currentDialSpeed + dialSpeedIncrement);

            Debug.Log($"Hit! Power: {_powerMeter:0.0} | Speed: {_currentDialSpeed:0.0} | Window: {_currentWindowSize:0.0}");
        }
        else
        {
            // Miss - penalty and window grows and moves
            float penalty = _powerMeter * missPenaltyPercent;
            _powerMeter = Mathf.Max(0f, _powerMeter - penalty);
            _totalMisses++;

            // Grow window
            _currentWindowSize = Mathf.Min(maxWindowSize, _currentWindowSize + windowGrowOnMiss);

            // Move window to random position 
            _currentWindowPosition = UnityEngine.Random.Range(0f, 360f);

            Debug.Log($"Miss! Penalty: {penalty:0.0} | New power: {_powerMeter:0.0} | Window moved to: {_currentWindowPosition:0.0}");
        }

        UpdateUI();
    }

    private bool IsAngleInWindow(float angle, float start, float end)
    {
        if (end > 360f)
        {
            return angle >= start || angle <= end - 360f;
        }
        return angle >= start && angle <= end;
    }

    private float GetWindowStartAngle()
    {
        return _currentWindowPosition - (_currentWindowSize * 0.5f);
    }

    private void UpdateDialVisual()
    {
        // Roatate needle
        if (dialNeedle != null) dialNeedle.localRotation = Quaternion.Euler(0f, 0f, -_dialAngle);

        // Update window size
        if (hitWindow != null)
        {
            // Rotate window to current position
            hitWindow.localRotation = Quaternion.Euler(0f, 0f, -_currentWindowPosition);

            // Update window arc visual
            Image windowImage = hitWindow.GetComponent<Image>();
            if (windowImage != null) windowImage.fillAmount = _currentWindowSize / 360f;
        }
    }

    private void UpdateUI()
    {
        // Update power meter
        if (powerMeterFill != null) powerMeterFill.fillAmount = Mathf.Clamp01(_powerMeter / powerDisplayCap);

        // Update timer
        if (timerText != null) timerText.text = $"{_timeRemaining:0.0}s";
    }

    private void EndMinigame()
    {
        _isActive = false;
        StopMinigameCoroutine();

        // Calculate score
        int finalScore = Mathf.RoundToInt(_powerMeter);
        string rating = GetRating(finalScore);

         // Show results
         if (scoreText != null) scoreText.text = $"{finalScore:0000}";
         if (ratingText != null) ratingText.text = rating;
         if (hitsText != null) hitsText.text = $"Hits: {_totalHits} | Misses: {_totalMisses}";
         if (resultsPanel != null) resultsPanel.SetActive(true);

         Debug.Log($"Minigame complete: Score: {finalScore} | Rating: {rating} | Hits: {_totalHits} | Misses: {_totalMisses}");
         OnMinigameComplete?.Invoke(finalScore);
    }

    private string GetRating(float score)
    {
        if (score >= 800) return "S+";
        if (score >= 700) return "S";
        if (score >= 600) return "A";
        if (score >= 300) return "B";
        if (score >= 200) return "C";
        if (score >= 100) return "D";
        return "F";
    }

    public override void StopMinigame()
    {
        _isActive = false;
        StopAllCoroutines();
        if (minigameUI != null) minigameUI.SetActive(false);
    }
}
