using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour, IDestructible
{
    public Stat maxHealth;
    public Stat currentHealth { get; private set; }

    public Stat Damage;
    public Stat Armor;

    public AttackDefinition defaultAttack;
    public EquipableItem weapon;
    public Shield shield;

    public bool isBlocking = false;

    public float attackCooldown;
    public float shieldCooldown;

    public bool isInvincible = false;
    public bool isDead = false;

    public event Action OnHealthValueChanged;

    public Transform WeaponBoneL;
    public Transform WeaponBoneR;

    protected virtual void Start()
    {
        currentHealth = new Stat();
        currentHealth.SetValue(maxHealth.GetValue());

        //weapon.OnItemEquipped(gameObject);
    }

    void Update()
    {
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;
        if (shieldCooldown > 0)
            shieldCooldown -= Time.deltaTime;
    }

    public void TakeDamage(GameObject attacker, float damage)
    {
        if (damage <= 0f) return;
        Debug.Log(attacker.name + this.gameObject.name);
        Debug.Log(currentHealth);
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
        if (weapon.weapon != null)
            return weapon.weapon;
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
