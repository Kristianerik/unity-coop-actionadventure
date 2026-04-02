using System.Collections;
using UnityEngine;

public class SpellBeam : SpellBase
{
    
    [Header("Beam Settings")]
    [SerializeField] private float beamLength = 15f;
    [SerializeField] private float tickDamage = 5f;
    [SerializeField] private float tickRate = 0.1f;
    [SerializeField] private LayerMask hitLayers;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public override void Cast(Vector3 position, Vector3 direction)
    {
        StartCoroutine(FireBeam(position, direction));
    }

    private IEnumerator FireBeam(Vector3 startPos, Vector3 direction)
    {
        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < duration) 
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            // Follow cast point and use its forward as aim direction
            if (_castPoint != null)
            {
                startPos = _castPoint.position;

                // Use camera aim direction 
                PlayerController playerController = _caster?.GetComponent<PlayerController>();

                direction = playerController != null ? playerController.GetAimDirection() : _castPoint.forward;
            }
            

            // Update beam visual every frame
            Vector3 endPos = startPos + direction * beamLength;
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, startPos);
                _lineRenderer.SetPosition(1, endPos);
            }

            // Deal tick damage
            if (tickTimer >= tickRate)
            {
                tickTimer = 0f;
                DamageAlongBeam(startPos, direction);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    public override void SetDamageMultiplier(float multiplier)
    {
        base.SetDamageMultiplier(multiplier);
        tickDamage *= multiplier;
    }

    private void DamageAlongBeam(Vector3 startPos, Vector3 direction)
    {
        RaycastHit[] hits = Physics.RaycastAll(startPos, direction, beamLength, hitLayers);

        foreach (var hit in hits)
        {
            HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();
            health?.TakeDamage(tickDamage, direction * 2f, _caster);
        }
    }
}
