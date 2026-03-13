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

    private int _currentAmmo;
    private float _fireRateTimer = 0f;
    private bool _isReloading = false;
    private bool _isAttacking = false;

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
        TryShoot(isCharged: true);
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

        TryShoot(isCharged);
    }

    private void Shoot(bool isCharged)
    {
        _currentAmmo--;
        _fireRateTimer = fireRate;

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            if (bullet.TryGetComponent<Bullet>(out var b))
            {
                float damage = isCharged ? heavyAttackDamage : lightAttackDamage;
                b.Initialize(Owner, damage, bulletSpeed);
            }
        }

        string trigger = isCharged ? "HeavyShot" : "LightShot";
        playerAnimator?.SetTrigger(trigger);
        Debug.Log($"ShotFired! Ammo: {_currentAmmo}/{maxAmmo}");
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
