using System.Collections;
using UnityEngine;

public class SpellBuff : SpellBase
{
    
    [Header("Buff Settings")]
    [SerializeField] private float fireRateMultiplier = 2f;
    [SerializeField] private float damageMultiplier = 1.5f;

    public override void Cast(Vector3 position, Vector3 direction)
    {
        StartCoroutine(ApplyBuff());
    }

    private IEnumerator ApplyBuff()
    {
        GunWeapon gun = _caster.GetComponentInChildren<GunWeapon>();
        TankComboSystem comboSystem = _caster.GetComponentInChildren<TankComboSystem>();

        // Apply buffs to both systems
        gun?.ApplyFireRateBuff(fireRateMultiplier);
        gun?.ApplyDamageBuff(damageMultiplier);
        comboSystem?.ApplyDamageBuff(damageMultiplier);

        Debug.Log($"All damage buffed {damageMultiplier}x | FireRate buffed {fireRateMultiplier}x for {duration}s!");

        yield return new WaitForSeconds(duration);

        // Remove buffs from both systems
        gun?.ApplyFireRateBuff(1f);
        gun?.ApplyDamageBuff(1f);
        comboSystem?.RemoveBuffs();

        Debug.Log("All buffs expired!");
    }
}
