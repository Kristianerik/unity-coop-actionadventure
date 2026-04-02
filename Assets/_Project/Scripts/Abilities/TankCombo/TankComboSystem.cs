using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class TankComboSystem : MonoBehaviour
{
    
    [Header("Combo System")]
    [SerializeField] private float comboInputWindow = 2f;
    [SerializeField] private int maxComboLength = 4;

    [Header("Spell Prefabs")]
    [SerializeField] private GameObject spellProjectilePrefab;
    [SerializeField] private GameObject spellAOEPrefab;
    [SerializeField] private GameObject spellShieldPrefab;
    [SerializeField] private GameObject spellBuffPrefab;
    [SerializeField] private GameObject spellBeamPrefab;

    [Header("References")]
    [SerializeField] private Transform castPoint;
    [SerializeField] private Animator playerAnimator;

    private List<int> _currentCombo = new List<int>();
    private float _comboTimer = 0f;
    private bool _isCasting = false;
    private float  _damageMultiplier = 1f;
    private GameObject _owner;

    // Combo definitions - sequence of button indices maps to spell
    private Dictionary<string, GameObject> _comboSpells;

    private void Awake()
    {
        _owner = gameObject;
        InitializeComboSpells();
    }

    private void InitializeComboSpells()
    {
        _comboSpells = new Dictionary<string, GameObject>
        {
          // 2 Button Combos
          {"1,2", spellProjectilePrefab}, // basic projectile
          {"2,1", spellAOEPrefab}, // AOE burst
          {"3,4", spellShieldPrefab}, // shield
          {"4,3", spellBuffPrefab}, // buff
          {"1,3", spellBeamPrefab}, // beam

          // 3 Button Combos
          {"1,2,3", spellProjectilePrefab}, // enhanced projectile
          {"2,3,4", spellAOEPrefab}, // enhanced AOE
          {"1,3,4", spellShieldPrefab}, // enhanced shield

          // 4 Button Combos
          {"1,2,3,4", spellProjectilePrefab}, // ultimate projectile
          {"4,3,2,1", spellAOEPrefab}, // ultimate aoe
          {"1,3,2,4", spellBeamPrefab} // ultimate beam  
        };
    }

    public void Initialize(GameObject owner)
    {
        _owner = owner;
    }

    private void Update()
    {
        if (_currentCombo.Count > 0)
        {
            _comboTimer += Time.deltaTime;
            if (_comboTimer >= comboInputWindow)
            {
                Debug.Log("Combat window expired - resetting");
                ResetCombo();
            }
        }
    }

    public void AddInput(int buttonIndex)
    {
        if (_isCasting) return;
        if (buttonIndex < 1 || buttonIndex > 4) return;

        _currentCombo.Add(buttonIndex);
        _comboTimer = 0f;

        Debug.Log($"Combo input: {GetComboString()} ({_currentCombo.Count}/{maxComboLength})");

        // Reset if max length reached with no valid combo
        if (_currentCombo.Count >= maxComboLength)
        {
            string comboKey = GetComboString();
            if (_comboSpells.ContainsKey(comboKey))
            {
                ConfirmCombo();
            }
            else
            {
                Debug.Log("Invalid combo - resetting");
                ResetCombo();
            }
        }
    }

    private IEnumerator CastSpell(GameObject spellPrefab, string comboKey)
    {
        _isCasting = true;

        Debug.Log($"Casting spell for combo: {comboKey}");
        playerAnimator?.SetTrigger("CastSpell");

        // Cast animation time scales with combo length
        float castTime = 0.3f + (_currentCombo.Count * 0.1f);
        yield return new WaitForSeconds(castTime);

        if (spellPrefab != null && castPoint != null)
        {
            // Get aim direction from player controller
            PlayerController playerController = _owner.GetComponent<PlayerController>();
            Vector3 aimDirection = playerController != null ? playerController.GetAimDirection() : _owner.transform.forward;

            // Rotate cast point to face aim direction
            Quaternion aimRotation = Quaternion.LookRotation(aimDirection);

            GameObject spell = Instantiate(spellPrefab, castPoint.position, aimRotation);

            SpellBase spellComponent = spell.GetComponent<SpellBase>();
            if (spellComponent != null)
            {
                float comboLengthMultiplier = 1f + ((_currentCombo.Count - 2) * 0.5f);

                spellComponent.Initialize(_owner);
                spellComponent.SetCastPoint(castPoint);
                spellComponent.SetDamageMultiplier(_damageMultiplier * comboLengthMultiplier);
                spellComponent.Cast(castPoint.position, aimDirection);
            }
        }

        ResetCombo();
        _isCasting = false;
    }

    public void ConfirmCombo()
    {
        if (_isCasting) return;
        if (_currentCombo.Count == 0) return;

        string comboKey = GetComboString();

        if (_comboSpells.ContainsKey(comboKey))
        {
            StartCoroutine(CastSpell(_comboSpells[comboKey], comboKey));
        }
        else
        {
            Debug.LogWarning($"No spell for combo: {comboKey} - resetting");
            ResetCombo();
        }
    }

    private string GetComboString()
    {
        return string.Join(",", _currentCombo);
    }

    private void ResetCombo()
    {
        _currentCombo.Clear();
        _comboTimer = 0f;
    }

    public void RemoveBuffs()
    {
        _damageMultiplier = 1f;
    }

    public void ApplyDamageBuff(float multiplier) => _damageMultiplier = multiplier;
    public float GetDamageMultiplier() => _damageMultiplier;
    public List<int> GetCurrentCombo() => new List<int>(_currentCombo);
    public bool isCasting() => _isCasting;
}
