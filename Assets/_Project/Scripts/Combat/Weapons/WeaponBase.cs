using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    
    [Header("Weapon Base Settings")]
    [SerializeField] protected String weaponName;
    [SerializeField] protected float lightAttackDamage = 10f;
    [SerializeField] protected float heavyAttackDamage = 25f;
    [SerializeField] protected Animator playerAnimator;

    protected GameObject Owner;

    public virtual void Initialize(GameObject owner, Animator animator)
    {
        Owner = owner;
        playerAnimator = animator;
    }

    public abstract void LightAttack(Vector2 moveInput);
    public abstract void HeavyAttack(Vector2 moveInput);
    public abstract void Block(bool isBlocking);
    public abstract bool IsAttacking();

    public virtual void CancelCharge() {}
    public virtual void OnEquip() {}
    public virtual void OnUnequip() {}
}
