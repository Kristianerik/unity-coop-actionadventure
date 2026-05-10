using System;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

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
    private GameObject minigameUI;
    private RectTransform dialNeedle;
    private RectTransform hitWindow;
    private Image powerMeterFill;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI hitsText;
    private TextMeshProUGUI ratingText;
    private TextMeshProUGUI bestStreakText;
    private TextMeshProUGUI exitPromptText;
    private GameObject resultsPanel;
    private float powerDisplayCap = 500f;

    [Header("Streak Settings")]
    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private int[] streakMilestones = { 5, 10, 15, 20 };
    [SerializeField] private Color[] streakColors = {
        Color.white,
        Color.yellow,
        new Color(1f, 0.5f, 0f), // orange
        Color.red
    };

    [Header("Miss Settings")]
    [SerializeField] private float missCooldown = 0.5f;
    [SerializeField] private float missShakeDuration = 0.3f;
    [SerializeField] private float missShakeMagnitude = 10f;
    [SerializeField] private float startupGraceDuration = .3f;
    private float _missCooldownTimer = 0f;
    private bool _inMissCooldown = false;
    private bool _startupGrace = false;
    private float _startupGraceTimer = 0f;

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
    private int _currentStreak = 0;
    private int _bestStreak = 0;

    private void Start()
    {
        if (minigameUI != null) FindUIReferences();

        if (minigameUI != null) minigameUI.SetActive(false);
    }

    private void FindUIReferences()
    {
        GameObject ui = GameObject.Find("PunchingBagUI");
        if (ui != null)
        {
            minigameUI = ui;
            dialNeedle = ui.transform.Find("Dial/DialNeedle") as RectTransform;
            hitWindow = ui.transform.Find("Dial/HitWindow") as RectTransform;
            powerMeterFill = ui.transform.Find("PowerMeter/PowerMeterFill").GetComponent<Image>();
            timerText = ui.transform.Find("TimerText").GetComponent<TextMeshProUGUI>();
            scoreText = ui.transform.Find("ResultsPanel/ScoreText").GetComponent<TextMeshProUGUI>();
            ratingText = ui.transform.Find("ResultsPanel/RatingText").GetComponent<TextMeshProUGUI>();
            hitsText = ui.transform.Find("ResultsPanel/HitsText").GetComponent<TextMeshProUGUI>();
            bestStreakText = ui.transform.Find("ResultsPanel/BestStreakText").GetComponent<TextMeshProUGUI>();
            exitPromptText = ui.transform.Find("ExitPromptText").GetComponent<TextMeshProUGUI>();
            streakText = ui.transform.Find("StreakText").GetComponent<TextMeshProUGUI>();
            resultsPanel = ui.transform.Find("ResultsPanel").gameObject;
        }
        else Debug.LogWarning("PunchingBagUI not found in scene!");
    }

    public void FindUIForPlayer(GameObject player)
    {
        // Find PunchingBagUI on this player's canvas
        Transform playerCanvas = player.transform.Find("PlayerCanvas");
        if (playerCanvas == null)
        {
            Debug.LogWarning("PlayerCanvas not found on player!");
            return;
        }

        Transform ui = playerCanvas.Find("PunchingBagUI");
        if (ui != null)
        {
            minigameUI = ui.gameObject;
            dialNeedle = ui.Find("Dial/DialNeedle") as RectTransform;
            hitWindow = ui.Find("Dial/HitWindow") as RectTransform;
            powerMeterFill = ui.transform.Find("PowerMeter/PowerMeterFill").GetComponent<Image>();
            timerText = ui.transform.Find("TimerText").GetComponent<TextMeshProUGUI>();
            scoreText = ui.transform.Find("ResultsPanel/ScoreText").GetComponent<TextMeshProUGUI>();
            ratingText = ui.transform.Find("ResultsPanel/RatingText").GetComponent<TextMeshProUGUI>();
            hitsText = ui.transform.Find("ResultsPanel/HitsText").GetComponent<TextMeshProUGUI>();
            bestStreakText = ui.transform.Find("ResultsPanel/BestStreakText").GetComponent<TextMeshProUGUI>();
            exitPromptText = ui.transform.Find("ExitPromptText").GetComponent<TextMeshProUGUI>();
            streakText = ui.transform.Find("StreakText").GetComponent<TextMeshProUGUI>();
            resultsPanel = ui.transform.Find("ResultsPanel").gameObject;

            Debug.Log($"UI found for player: {player.name}");
        } 
        else Debug.LogWarning($"PunchingBagUI not found on {player.name}!");    
        
    }

    public override void StartMinigame()
    {
        _isActive = true;
        _dialAngle = 0f;
        _currentDialSpeed = initialDialSpeed;
        _currentWindowSize = initialWindowSize;
        _currentWindowPosition = (_dialAngle + 45f) % 360f;
        _powerMeter = 0f;
        _timeRemaining = gameDuration;
        _totalHits = 0;
        _totalMisses = 0;
        _inMissCooldown = false;
        _missCooldownTimer = 0f;
        _startupGrace = true;
        _startupGraceTimer = startupGraceDuration;


        if (minigameUI != null) minigameUI.SetActive(true);
        if (exitPromptText != null) exitPromptText.text = "Press ESCAPE to exit";
        if (resultsPanel != null) resultsPanel.SetActive(false);

        UpdateUI();
        StartMinigameCoroutine(RunMinigame());
    }

    private IEnumerator RunMinigame()
    {
        while (_timeRemaining > 0 && _isActive)
        {
            _timeRemaining -= Time.deltaTime;

            // Tick startup grace
            if (_startupGrace)
            {
                _startupGraceTimer -= Time.deltaTime;
                if (_startupGraceTimer <= 0f) _startupGrace = false;
            }

            // Tick miss cooldown
            if (_inMissCooldown)
            {
                _missCooldownTimer -= Time.deltaTime;
                if (_missCooldownTimer <= 0f) _inMissCooldown = false;
            }

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
        if (_startupGrace) return;  

        // Sample current angle at exact moment of input
        float windowStart = GetWindowStartAngle();
        float windowEnd = windowStart + _currentWindowSize;
        bool inWindow = IsAngleInWindow(_dialAngle, windowStart, windowEnd);


        if (inWindow)
        {
            // Reset miss cooldown on successful hit
            _inMissCooldown = false;
            _missCooldownTimer = 0f;

            // Hit in window - build power
            _powerMeter += hitPower;
            _totalHits++;

            // Shrink window and speed up needle
            _currentWindowSize = Mathf.Max(minWindowSize, _currentWindowSize - windowShrinkOnHit);
            _currentDialSpeed = Mathf.Min(maxDialSpeed, _currentDialSpeed + dialSpeedIncrement);
            UpdateStreak(true);
        }
        else
        {
            // Only apply penalty if not in grace period
            if (!_inMissCooldown)
            {
                // Miss - penalty and window grows and moves
                float penalty = _powerMeter * missPenaltyPercent;
                _powerMeter = Mathf.Max(0f, _powerMeter - penalty);
                _totalMisses++;

                // Grow window
                _currentWindowSize = Mathf.Min(maxWindowSize, _currentWindowSize + windowGrowOnMiss);

                // Move window to random position 
                ResetWindowPosition();
                UpdateStreak(false);

                // Start grace period
                _inMissCooldown = true;
                _missCooldownTimer = missCooldown;

                StartCoroutine(ShakeDial());

                Debug.Log($"Miss! Penalty: {penalty:0.0} | New power: {_powerMeter:0.0} | Window moved to: {_currentWindowPosition:0.0}");
            } 
            else
            {
                Debug.Log("Miss ingorned - in grace period");
            }
        }

        UpdateUI();
    }

    private void UpdateStreak(bool hit)
    {
        if (hit)
        {
            _currentStreak++;
            _bestStreak = Mathf.Max(_bestStreak, _currentStreak);
            UpdateStreakUI();
        }
        else
        {
            _currentStreak = 0;
            if (streakText != null)
            {
                streakText.text = "";
                streakText.transform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateStreakUI()
    {
        if (streakText == null) return;

        // Find which milestone we're at
        int milestoneIndex = 0;
        for (int i = streakMilestones.Length - 1; i >= 0; i--)
        {
            if (_currentStreak >= streakMilestones[i])
            {
                milestoneIndex = i;
                break;
            }
        }

        // Update text
        streakText.text = $"x{_currentStreak} STREAK!";

        // Update color based on milestone
        streakText.color = streakColors[Mathf.Min(
            milestoneIndex, streakColors.Length - 1)];

        // Scale up at milestones
        float targetScale = 1f + (milestoneIndex * 0.15f);
        streakText.transform.localScale = Vector3.one * targetScale;

        // Shake at milestones
        if (_currentStreak % 5 == 0)
            StartCoroutine(ShakeText(streakText));
    }

    private IEnumerator ShakeDial()
    {
        if (dialNeedle == null) 
        { 
            Debug.LogError("dialNeedle is null!"); 
            yield break; 
        }

        Transform dialTransform = dialNeedle.parent;
        Vector3 originalPos = dialTransform.localPosition;

        // Flash background red
        Image bgImage = dialTransform.Find("DialBackground").GetComponent<Image>();
        Color originalColor = bgImage != null ? bgImage.color : Color.white;

        if (bgImage != null)
            bgImage.color = Color.red;

        float elapsed = 0f;
        while (elapsed < missShakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + Random.Range(-missShakeMagnitude, missShakeMagnitude);
            float y = originalPos.y + Random.Range(-missShakeMagnitude, missShakeMagnitude);
            dialTransform.localPosition = new Vector3(x, y, originalPos.z);
            yield return null;
        }

        dialTransform.localPosition = originalPos;

        // Restore background color
        if (bgImage != null)
            bgImage.color = originalColor;
    }

    private IEnumerator ShakeText(TextMeshProUGUI text)
    {
        Vector3 originalPos = text.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.3f;
        float magnitude = 5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + UnityEngine.Random.Range(-magnitude, magnitude);
            float y = originalPos.y + UnityEngine.Random.Range(-magnitude, magnitude);
            text.transform.localPosition = new Vector3(x, y, originalPos.z);
            yield return null;
        }

        text.transform.localPosition = originalPos;
    }

    private bool IsAngleInWindow(float angle, float start, float end)
    {
        // Normalize all angles to 0-360
        angle = (angle % 360f + 360f) % 360f;
        start = (start % 360f + 360f) % 360f;
        end = (end % 360f + 360f) % 360f;

        if (end < start) return angle >= start || angle <= end;
        return angle >= start && angle <= end;
    }

    private float GetWindowStartAngle()
    {
        float half = _currentWindowSize * 0.5f;
        return (_currentWindowPosition - half + 360f) % 360f;
    }

    private void ResetWindowPosition()
    {
        float attempts = 0;
        float newPosition;

        do
        {
            newPosition = Random.Range(0f, 360f);
            attempts++;

            // Check if needle is inside new position
            float start = (newPosition - _currentWindowSize * 0.5f + 360f) % 360f;
            float end = (newPosition + _currentWindowSize * 0.5f) % 360f;
            bool needleInside = IsAngleInWindow(_dialAngle, start, end);

            if (!needleInside || attempts > 10)
            {
                _currentWindowPosition = newPosition;
                return;
            }

        } while (attempts <= 10);

        // Fallback: place window opposite to needle
        _currentWindowPosition = (_dialAngle + 180f) % 360f;
    }

    private void UpdateDialVisual()
    {
        // Roatate needle
        if (dialNeedle != null) dialNeedle.localRotation = Quaternion.Euler(0f, 0f, -_dialAngle);

        // Update window size
        if (hitWindow != null)
        {
            // Change minus to plus for correct direction
            float rotationOffset = _currentWindowSize * 0.5f;
            hitWindow.localRotation = Quaternion.Euler(
                0f, 0f, -(_currentWindowPosition + rotationOffset));

            Image windowImage = hitWindow.GetComponent<Image>();
            if (windowImage != null)
                windowImage.fillAmount = _currentWindowSize / 360f;
        }
    }

    private void UpdateUI()
    {
        if (powerMeterFill != null)
        {
            float fillAmount = Mathf.Clamp01(_powerMeter / powerDisplayCap);
            powerMeterFill.fillAmount = fillAmount;

            Color meterColor;
            if (fillAmount < 0.5f)
            {
                // Yellow to orange (0% to 50%)
                meterColor = Color.Lerp(
                    Color.yellow,
                    new Color(1f, 0.5f, 0f), 
                    fillAmount * 2f
                );
            }
            else
            {
                // Orange to red (50% to 100%)
                meterColor = Color.Lerp(
                    new Color(1f, 0.5f, 0f), 
                    Color.red,
                    (fillAmount - 0.5f) * 2f
                );
            }

            powerMeterFill.color = meterColor;
        }

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
         if (bestStreakText != null) bestStreakText.text = $"Best Streak: {_bestStreak}";
         if (resultsPanel != null) resultsPanel.SetActive(true);

         Debug.Log($"Minigame complete: Score: {finalScore} | Rating: {rating} | Hits: {_totalHits} | Misses: {_totalMisses} | Best Streak: {_bestStreak}");
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
        StopMinigameCoroutine();
        if (minigameUI != null) minigameUI.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }
}
