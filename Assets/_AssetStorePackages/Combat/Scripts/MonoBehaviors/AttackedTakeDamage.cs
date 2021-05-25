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

        if (stats.GetCurrentHealth().GetValue() <= 0)
        {
            if (gameObject.layer == 8)
            {
                if (GetComponent<NPC>().enabled)
                {
                    GetComponent<NPC>().OnDestruction(attacker);
                }
                else if (GetComponent<ShieldMeleeAI>().enabled)
                {
                    GetComponent<ShieldMeleeAI>().OnDestruction(attacker);
                }
            }
        }
    }
}
