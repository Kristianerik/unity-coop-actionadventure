using UnityEngine;

public class HealthRegeneration : MonoBehaviour
{
    
    [Header("Regeneration Settings")]
    [SerializeField] private float regenRate = 5f;
    [SerializeField] private float regenDelay = 5f;
    [SerializeField] private bool regenEnabled = true;

    private HealthSystem _healthSystem;
    private float _regenDelayTimer = 0f;
    private bool _isRegenActive = false;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        _healthSystem.OnDamageTaken += _ => ResetRegenDelay();
    }

    private void Update()
    {
        if (!regenEnabled) return;
        if (_healthSystem.IsDead()) return;
        if(_healthSystem.GetHealthPercent() >= 1f) return;

        if (!_isRegenActive)
        {
            _regenDelayTimer += Time.deltaTime;
            if (_regenDelayTimer >= regenDelay) _isRegenActive = true;
        }
        else 
        {
            _healthSystem.Heal(regenRate * Time.deltaTime);
        }
    }

    private void ResetRegenDelay()
    {
        _regenDelayTimer = 0f;
        _isRegenActive = false;
    }
}
