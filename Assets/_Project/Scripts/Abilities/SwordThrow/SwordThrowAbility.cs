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

        // Spawn thrown sword
        GameObject thrown = Instantiate(thrownSwordPrefab, transform.position + _owner.transform.forward, _owner.transform.rotation);

        ThrownSword thrownSword = thrown.GetComponent<ThrownSword>();
        if (thrownSword != null)
        {
            thrownSword.Initialize(_owner, throwDamage, throwSpeed, maxRange, hitLayers);
            thrownSword.OnReturnedToOwner += OnSwordReturned;
        }
    }

    private void OnSwordReturned()
    {
        _swordOut = false;

        // Show sword on player
        if (swordVisual != null) swordVisual.SetActive(true);

        Debug.Log("Sword returned!");
    }

    public override bool CanUse() => base.CanUse() && !_swordOut;
}
