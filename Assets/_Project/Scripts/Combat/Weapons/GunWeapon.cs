using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class GunWeapon : WeaponBase
{
    [Header("Gun Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Charged Shot")]
    [SerializeField] private float chargeTime = 0.8f;
    [SerializeField] private float chargedDamageMultiplier = 3f;
    [SerializeField] private float chargedBulletScale = 2f;

    private int _currentAmmo;
    private float _fireRateTimer = 0f;
    private bool _isReloading = false;
    private bool _isAttacking = false;
    private bool _isCharging = false;
    private float _chargeTimer = 0f;
    private float _chargePercent = 0f;

    private void Awake()
    {
        _currentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (_fireRateTimer > 0f)
        {
            _fireRateTimer -= Time.deltaTime;
        }
    }

    public override void LightAttack(Vector2 moveInput)
    {
        TryShoot();
    }

    public override void HeavyAttack(Vector2 moveInput)
    {
        if (!_isCharging && !_isReloading && _fireRateTimer <= 0f)
        {
            _isCharging = true;
            _chargeTimer = 0f;
            _chargePercent = 0f;
            StartCoroutine(ChargeShot());
            Debug.Log("Charging shot...");
        }
    }

    private IEnumerator ChargeShot()
    {
        while (_isCharging)
        {
            _chargeTimer += Time.deltaTime;
            _chargePercent = Mathf.Clamp01(_chargeTimer / chargeTime);
            Debug.Log($"Charge: {(_chargePercent * 100f):0}%");

            // Auto fire at full charge
            if (_chargePercent >= 1f)
            {
                _isCharging = false;
                TryShoot(isCharged: true);
                _chargeTimer = 0f;
                _chargePercent = 0f;
                Debug.Log("Auto fired at full charge!");
                yield break; // exit the coroutine
            }

            yield return null;
        }
    }

    public override void CancelCharge()
    {
        Debug.Log("GunWeapon CancelCharge called - isCharging: " + _isCharging);
        if (!_isCharging) return;

        _isCharging = false;
        TryShoot(isCharged: true);
        _chargeTimer = 0f;
        _chargePercent = 0f;
    }

    public override void Block(bool isBlocking)
    {
        // TODO: Implement blocking logic (e.g., reduce incoming damage, play block animation)
        playerAnimator?.SetBool("IsBlocking", isBlocking);
    }

    public override bool IsAttacking() => _isAttacking;

    private void TryShoot(bool isCharged = false)
    {
        if (_isReloading) { return; }
        if (_fireRateTimer > 0f) { return; }
        if (_currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Shoot(isCharged);
    }

    private void Shoot(bool isCharged)
    {
        _currentAmmo--;
        _fireRateTimer = fireRate;

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            float damage;
            float scale;

            if (isCharged)
            {
                // Scale damage and size based on charge percent
                damage = Mathf.Lerp(
                    lightAttackDamage,
                    lightAttackDamage * chargedDamageMultiplier,
                    _chargePercent
                );
                scale = Mathf.Lerp(1f, chargedBulletScale, _chargePercent);
                bullet.transform.localScale *= scale;
                Debug.Log($"Shot fired! Charge: {(_chargePercent * 100f):0}% | Damage: {damage:0.0} | Scale: {scale:0.00}");
            }
            else
            {
                damage = lightAttackDamage;
                Debug.Log($"Normal shot fired! Damage: {damage} | Ammo: {_currentAmmo}/{maxAmmo}");
            }

            if (bullet.TryGetComponent<Bullet>(out var b))
            {
                b.Initialize(Owner, damage, bulletSpeed);
            }
        }

        string trigger = isCharged ? "HeavyShot" : "LightShot";
        playerAnimator?.SetTrigger(trigger);
    }

    private System.Collections.IEnumerator Reload()
    {
        _isReloading = true;
        playerAnimator?.SetTrigger("Reload");
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        _currentAmmo = maxAmmo;
        _isReloading = false;
        Debug.Log("Reload complete!");
    }

    public override void OnEquip()
    {
        gameObject.SetActive(true);
    }

    public override void OnUnequip()
    {
        gameObject.SetActive(false);
    }

    public int GetCurrentAmmo() => _currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsReloading() => _isReloading;

}
