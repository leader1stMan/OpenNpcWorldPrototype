using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class AttackedTakeDamage : MonoBehaviour, IAttackable
{
    private CharacterStats stats;
    public bool RagdollOnDeath = true;
    void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void OnAttack(GameObject attacker, Attack attack)
    {
        stats.TakeDamage(attacker, attack.Damage);
        CombatBase[] combatScripts = GetComponents<CombatBase>();
        foreach (CombatBase combatScript in combatScripts)
        {
            if (combatScript.enabled)
            {
                combatScript.currentTarget = attacker.transform;
            }
        }

        if (stats.GetCurrentHealth().GetValue() <= 0)
        {
            if (gameObject.layer == 8)
            {
                GetComponent<NPC>().OnDestruction(attacker);
            }
        }
    }
}
