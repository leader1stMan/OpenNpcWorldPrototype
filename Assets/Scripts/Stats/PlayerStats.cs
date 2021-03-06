﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
     protected override void Start()
    {
        base.Start();
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
                weapon = newItem.weapon;
            if (newItem.shield != null)
                shield = newItem.shield;
        }
    }
}
