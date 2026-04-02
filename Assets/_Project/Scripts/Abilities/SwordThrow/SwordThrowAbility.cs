using Unity.VisualScripting;
using UnityEngine;

public class SwordThrowAbility : AbilityBase
{
    
    [Header("Sword Throw Settings")]
    [SerializeField] private float throwDamage = 25f;
    [SerializeField] private float throwSpeed = 15f;
    [SerializeField] private float maxRange = 10f;
    [SerializeField] private GameObject swordVisual;
    [SerializeField] private GameObject thrownSwordPrefab;
    [SerializeField] private LayerMask hitLayers;

    private bool _swordOut = false;

    protected override void Execute()
    {
        if (_swordOut) return;

        _swordOut = true;

        // Hide sword on player
        if (swordVisual != null) swordVisual.SetActive(false);

        // Get aim direction from player controller
        PlayerController playerController = _owner.GetComponent<PlayerController>();
        Vector3 aimDirection = playerController != null ? playerController.GetAimDirection() : _owner.transform.forward;

        Vector3 spawnPosition = _owner.transform.position + Vector3.up * 1.2f + aimDirection;

        // Spawn thrown sword
        GameObject thrown = Instantiate(thrownSwordPrefab, spawnPosition, Quaternion.LookRotation(aimDirection));

        ThrownSword thrownSword = thrown.GetComponent<ThrownSword>();
        if (thrownSword != null)
        {
            thrownSword.Initialize(_owner, throwDamage, throwSpeed, maxRange, hitLayers, 1.2f, aimDirection);
            thrownSword.OnReturnedToOwner -= OnSwordReturned;
            thrownSword.OnReturnedToOwner += OnSwordReturned;
        }
    }

    private void OnSwordReturned()
    {
        _swordOut = false;

        // Show sword on player
        if (swordVisual != null) swordVisual.SetActive(true);
    }

    public override bool CanUse() => base.CanUse() && !_swordOut;
}
