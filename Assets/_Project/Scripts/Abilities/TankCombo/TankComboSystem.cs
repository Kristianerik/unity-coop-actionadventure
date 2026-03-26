using System.Collections;
using System.Collections.Generic;
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
          {"1,2,3,4", spellBeamPrefab}, // ultimate beam
          {"4,3,2,1", spellAOEPrefab} // ultimate aoe 
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

        // Check for valid combo
        string comboKey = GetComboString();
        if (_comboSpells.ContainsKey(comboKey))
        {
            // Valid combo begin cast
            StartCoroutine(CastSpell(_comboSpells[comboKey], comboKey));
        }
        else if (_currentCombo.Count >= maxComboLength)
        {
            // Max length reached with no valid combo
            Debug.Log("Invalid combo - resetting");
            ResetCombo();
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
            GameObject spell = Instantiate(spellPrefab, castPoint.position, castPoint.rotation);

            SpellBase spellComponent = spell.GetComponent<SpellBase>();
            if (spellComponent != null)
            {
                spellComponent.Initialize(_owner);
                spellComponent.SetDamageMultiplier(_damageMultiplier);
                spellComponent.Cast(castPoint.position, _owner.transform.forward);
            }
        }

        ResetCombo();
        _isCasting = false;
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
