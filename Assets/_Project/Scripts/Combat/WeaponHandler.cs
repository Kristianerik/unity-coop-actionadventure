using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    
    [Header("Equipped Weapon")]
    [SerializeField] private WeaponBase equippedWeapon;
    [SerializeField] private Animator playerAnimator;

    private void Start()
    {
        if (equippedWeapon != null) {
            equippedWeapon.Initialize(gameObject, playerAnimator);
        }
    }

    public void LightAttack(Vector2 moveInput)
    {
        equippedWeapon?.LightAttack(moveInput);
    }

    public void HeavyAttack(Vector2 moveInput)
    {
        equippedWeapon?.HeavyAttack(moveInput);
    }

    public void Block(bool isBlocking)
    {
        equippedWeapon?.Block(isBlocking);
    }

    public bool IsAttacking() => equippedWeapon?.IsAttacking() ?? false;

    public void EquipWeapon(WeaponBase newWeapon)
    {
        equippedWeapon?.OnUnequip();
        equippedWeapon = newWeapon;
        equippedWeapon?.Initialize(gameObject, playerAnimator);
        equippedWeapon?.OnEquip();
    }
    
}
