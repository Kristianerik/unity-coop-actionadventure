using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    
    [Header("Spell Settings")]
    [SerializeField] protected string spellName;
    [SerializeField] protected float damage = 20f;
    [SerializeField] protected float duration = 3f;
    [SerializeField] protected float castTime = 3f;

    protected float _damageMultiplier = 1f;

    protected GameObject _caster;

    public virtual void Initialize(GameObject caster)
    {
        _caster = caster;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        _damageMultiplier = multiplier;
        damage *= multiplier;
    }

    public abstract void Cast(Vector3 position, Vector3 direction);
    public string GetSpellName() => spellName;
    public float GetCastTime() => castTime;
}
