using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    void Start()
    {
        EquipmentController.instance.onEquipmentChanged += OnEquipmentChanged;
    }

    void OnEquipmentChanged(EquipableItem newItem, EquipableItem oldItem)
    {
        if (newItem != null)
        {
            Armor.AddModifier(newItem.armorModifier);
            Damage.AddModifier(newItem.damageModifier);

            if (newItem.weapon != null)
                weapon = newItem.weapon;
            if (newItem.shield != null)
                shield = newItem.shield;
        }

        if (oldItem != null)
        {
            Armor.RemoveModifier(oldItem.armorModifier);
            Damage.RemoveModifier(oldItem.damageModifier);
            weapon = null;
            shield = null;
        }
    }
}
