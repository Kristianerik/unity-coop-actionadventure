using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public enum AttackType { Light, Heavy, Either }

[System.Serializable]
public class ComboAttack
{
    public string attackName;
    public AttackType requiredInput;    
    public float damage;
    public float knockbackForce;
    public float duration;
    public float comboWindowStart;
    public float comboWindowEnd;
    public Vector2 requiredDirection;
    public string animationTrigger;
    public AudioClip attackSound;
}

[System.Serializable]
public class ComboChain
{
    public string chainName;
    public List<ComboAttack> attacks = new List<ComboAttack>();

    // Check if the next attack in the chain matches the requested input
    public bool MatchesNextInput(int index, AttackType input)
    {
        if (index >= attacks.Count) return false;
        return attacks[index].requiredInput == input ||
               attacks[index].requiredInput == AttackType.Either;
    }
}
