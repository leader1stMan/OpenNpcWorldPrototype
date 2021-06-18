using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour, IDestructible
{
    public Stat maxHealth;
    public Stat currentHealth { get; private set; }

    public Stat Damage;
    public Stat Armor;

    public AttackDefinition defaultAttack;
    public Weapon weapon;
    public Shield shield;
    public bool isBlocking;
    public float attackCooldown;

    public bool isDead;

    public event Action OnHealthValueChanged;

    protected virtual void Start()
    {
        currentHealth = new Stat();
        currentHealth.SetValue(maxHealth.GetValue());
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    public void TakeDamage(GameObject attacker, float damage)
    {
        if (damage <= 0f) return;
        
        currentHealth.SetValue(currentHealth.GetValue() - damage);

        OnHealthValueChanged?.Invoke();
    }

    public Stat GetArmor()
    {
        return Armor;
    }

    public Stat GetDamage()
    {
        return Damage;
    }

    public Stat GetCurrentHealth()
    {
        return currentHealth;
    }

    public Stat GetMaxHealth()
    {
        return maxHealth;
    }
    public Weapon GetWeapon()
    {
        if (weapon != null)
            return weapon;
        else
            return null;
    }
    public Shield GetShield()
    {
        if (shield != null)
            return shield;
        else
            return null;
    }

    public void OnDestruction(GameObject destroyer)
    {
        isDead = true;
    }
}
