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
        if (oldItem != null)
        {
            Armor.RemoveModifier(oldItem.armorModifier);
            Damage.RemoveModifier(oldItem.damageModifier);

            if (oldItem.weapon != null)
                weapon = null;
            if (oldItem.shield != null)
                shield = null;
        }
        if (newItem != null)
        {
            Armor.AddModifier(newItem.armorModifier);
            Damage.AddModifier(newItem.damageModifier);

            if (newItem.weapon != null)
                weapon = newItem;
            if (newItem.shield != null)
                shield = newItem.shield;
        }
    }
}
