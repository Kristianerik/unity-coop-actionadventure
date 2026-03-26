using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SpellAOE : SpellBase
{
    
    [Header("AOE Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float expandSpeed = 8f;
    [SerializeField] private LayerMask enemyLayer;

    private bool _hasDealthDamage = false;

    public override void Cast(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        StartCoroutine(ExpandAndDamage()); 
    }

    private IEnumerator ExpandAndDamage()
    {
        float currentRadius = 0f;

        while (currentRadius < radius)
        {
            currentRadius += expandSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * currentRadius * 2f;

            if (!_hasDealthDamage && currentRadius >= radius * 0.5f)
            {
                _hasDealthDamage = true;
                DamageEnemiesInRadius();
            }

            yield return null;
        }

        Destroy(gameObject, 0.5f);
    }

    private void DamageEnemiesInRadius()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        foreach (var hit in hits)
        {
            HealthSystem health = hit.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                Vector3 knockback = (hit.transform.position - transform.position).normalized * 8f;
                health.TakeDamage(damage, knockback, _caster);
            }
        }

        Debug.Log($"AOE hit {hits.Length} enemies!");
    }
}


