using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack.asset", menuName = "Attack/BaseAttack")]
public class AttackDefinition : ScriptableObject
{
    public float Cooldown;

    public float minDamage;
    public float maxDamage;
    public float criticalMultipliyer;
    public float criticalChance;
    public float Range;

    public Attack CreateAttack(CharacterStats attacker, CharacterStats defender)
    {
        float baseDamage = attacker.GetDamage().GetValue();
        baseDamage += Random.Range(minDamage, maxDamage);

        bool isCritical = Random.value < criticalChance;
        if (isCritical)
            baseDamage *= criticalMultipliyer;

        if (defender != null)
            baseDamage -= defender.GetArmor().GetValue();

        if (baseDamage < 0)
            baseDamage = 0;
        return new Attack((int)baseDamage, isCritical);
    }
}
