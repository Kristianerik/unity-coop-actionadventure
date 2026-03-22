using UnityEngine;

public class ArenaHazard : MonoBehaviour
{
    
    [Header("Hazard Settings")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float warningDuration = 1.5f;

    private bool _isActive = false;
    private float _damageTimer = 0f;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        StartCoroutine(WarningThenActivate());
    }

    public void Deactivate()
    {
        _isActive =false;
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator WarningThenActivate()
    {
        // Warning phase - flash yellow
        _isActive = false;
        float warningTimer = 0f;
        while (warningTimer < warningDuration)
        {
            warningTimer += Time.deltaTime;
            if (_renderer != null)
            {
                _renderer.material.color = Color.Lerp(Color.yellow, Color.red, warningTimer / warningDuration);
                yield return null;
            }
        }

        // Active phase - deal damage
        _isActive = true;
        if (_renderer != null)
        {
            _renderer.material.color = Color.red;
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        _damageTimer += Time.deltaTime;
        if (_damageTimer >= damageInterval)
        {
            _damageTimer = 0f;
            DamagePlayersInHazard();
        }
    }

    private void DamagePlayersInHazard()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale * 0.5f, transform.rotation);

        foreach (var hit in hits)
        {
            HealthSystem health = hit.GetComponentInParent<HealthSystem>();
            health?.TakeDamage(damage, Vector3.zero);
        }
    }
}
