using UnityEngine;

public class SwordWeapon : WeaponBase
{
    
    [Header("Sword Settings")]
    [SerializeField] private HitboxController hitbox;
    [SerializeField] private ComboSystem comboSystem;

    public override void Initialize(GameObject owner, Animator animator)
    {
        base.Initialize(owner, animator);
        hitbox?.Initialize(owner);
        comboSystem?.Initialize(hitbox, animator);
    }
    public override void LightAttack(Vector2 moveInput)
    {
        comboSystem?.RequestLightAttack(moveInput);
    }

    public override void HeavyAttack(Vector2 moveInput)
    {
        comboSystem?.RequestHeavyAttack(moveInput);
    }

    public override void Block(bool isBlocking)
    {
        // TODO: Implement blocking logic (e.g., reduce incoming damage, play block animation)
        playerAnimator?.SetBool("IsBlocking", isBlocking);
    }

    public override bool IsAttacking() => comboSystem?.IsAttacking() ?? false;

    public override void OnEquip()
    {
        gameObject.SetActive(true);
    }

    public override void OnUnequip()
    {
        gameObject.SetActive(false);
    }
    
}
