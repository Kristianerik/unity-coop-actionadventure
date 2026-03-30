using System.Collections;
using UnityEngine;

public class DamageVignette : MonoBehaviour
{
    
    [Header("Vignette Settings")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float maxAlpha = 0.5f;
    [SerializeField] private Color damageColor = Color.red;

    private CanvasGroup _canvasGroup;
    private UnityEngine.UI.Image _vignetteImage;
    private bool _isFlashing = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _vignetteImage = GetComponent<UnityEngine.UI.Image>();

        if (_vignetteImage != null) _vignetteImage.color = damageColor;
    }

    public void TriggerFlash()
    {
        if (!_isFlashing) StartCoroutine(FlashVignette());
    }

    private IEnumerator FlashVignette()
    {
        _isFlashing = true;
        float timer = 0f;

        // Fade in
        while (timer < flashDuration * 0.5f)
        {
            timer += Time.deltaTime;
            if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Lerp(0, maxAlpha, timer / (flashDuration * 0.5f));
            yield return null;
        }

        // Fade out
        timer = 0f;
        while (timer < flashDuration * 0.5f)
        {
            timer += Time.deltaTime;
            if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Lerp(maxAlpha, 0, timer / (flashDuration * 0.5f));
            yield return null;
        }

        if (_canvasGroup != null) _canvasGroup.alpha = 0;
        _isFlashing = false;
    }
}
