using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class AttackedTakeDamage : MonoBehaviour, IAttackable
{
    private CharacterStats stats;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void OnAttack(GameObject attacker, Attack attack)
    {
        stats.TakeDamage(attacker, attack.Damage);

        if (stats.GetCurrentHealth().GetValue() <= 0)
        {
            var destructibles = GetComponents(typeof(IDestructible));
            foreach (IDestructible d in destructibles)
            {
                d.OnDestruction(attacker);
            }
        }
    }
}
