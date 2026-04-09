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
    [SerializeField] private float dialRotationSpeed = 90f;
    [SerializeField] private float initialWindowSize = 60f;
    [SerializeField] private float windowShrinkRate = 2f;
    [SerializeField] private float minWindowSize = 15f;
    [SerializeField] private float hitPower = 15f;
    [SerializeField] private float gameDuration = 45f;
    [SerializeField] private TextMeshProUGUI timerText;
    private float _timeRemaining;
    private int _totalHits = 0;

    [Header("UI References")]
    [SerializeField] private GameObject minigameUI;
    [SerializeField] private RectTransform dialNeedle;
    [SerializeField] private RectTransform hitWindow;
    [SerializeField] private Image powerMeterFill;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hitsText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private float powerDisplayCap = 500f;

    // State
    private bool _isActive = false;
    private float _dialAngle = 0f;
    private float _currentWindowSize;
    private float _powerMeter = 0f;

    private void Start()
    {
        if (minigameUI != null) minigameUI.SetActive(false);
    }

    public override void StartMinigame()
    {
        _isActive = true;
        _dialAngle = 0f;
        _currentWindowSize = initialWindowSize;
        _powerMeter = 0f;
        _timeRemaining = gameDuration;

        if (minigameUI != null) minigameUI.SetActive(true);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        UpdateUI();
        StartMinigameCoroutine(RunMinigame());
    }

    private IEnumerator RunMinigame()
    {
        while (_timeRemaining > 0 && _isActive)
        {
            // Rotate dial
            _dialAngle += dialRotationSpeed * Time.deltaTime;
            if (_dialAngle >= 360f) _dialAngle -= 360f;

            // Shrink window over time
            _currentWindowSize = Mathf.Max(minWindowSize, _currentWindowSize - windowShrinkRate * Time.deltaTime);

            UpdateDialVisual();
            yield return null;
        }

        EndMinigame();
    }

    public override void OnHit()
    {
        if (!_isActive) return;
        TryHit(hitPower);
    }

    

    private void TryHit(float power)
    {
        // Check if dial is in window
        float windowStart = GetWindowStartAngle();
        float windowEnd= windowStart +_currentWindowSize;
        bool inWindow = IsAngleInWindow(_dialAngle, windowStart, windowEnd);

        if (inWindow)
        {
            // Scale power by how centered the hit is
            float windowCenter = windowStart + (_currentWindowSize * 0.5f);
            float distanceFromCenter = Mathf.Abs(_dialAngle - windowCenter);
            float accuracy = 1f - (distanceFromCenter / (_currentWindowSize * 0.5f));
            float actualPower = power * accuracy;
            _powerMeter += actualPower;
            _totalHits++;
            Debug.Log($"Hit! Power: {actualPower:0.0} | Accuracy: {accuracy: 0.0%}");
        }
        else
        {
            Debug.Log("Miss!");
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
        // Window pos is fixed at top of dial 270 degrees
        return 270f - (_currentWindowSize * 0.5f);
    }

    private void UpdateDialVisual()
    {
        // Roatate needle
        if (dialNeedle != null) dialNeedle.localRotation = Quaternion.Euler(0f, 0f, -_dialAngle);

        // Update window size
        if (hitWindow != null)
        {
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
         if (hitsText != null) hitsText.text = $"Hits: {_totalHits}";
         if (resultsPanel != null) resultsPanel.SetActive(true);

         Debug.Log($"Minigame complete: Score: {finalScore} | Rating: {rating} | Hits: {_totalHits}");
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
