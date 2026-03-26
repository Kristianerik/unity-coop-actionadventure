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

            // Update beam visual
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
