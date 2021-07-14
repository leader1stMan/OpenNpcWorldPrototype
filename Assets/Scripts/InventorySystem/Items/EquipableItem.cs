using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipableItem", menuName = "Items/EquipableItem")] 
public class EquipableItem : Item
{
    public EquipTypes EquipType;

    public float damageModifier;
    public float armorModifier;
    public float healthModifier;

    public Weapon weapon = null;
    public Shield shield = null;

    override public void OnItemUsed(){
        
    }

    override public void OnItemEquipped(AnimationController controller)
    {
        EquipmentController.instance.Equip(this);
        if (weapon != null && weapon.type == WeaponType.LowRange)
        { 
            controller.ChangeAnimation(AnimationController.SWORD_EQUIP, AnimatorLayers.UP, true);
        }
    }

    override public void OnItemUnEquipped()
    {
        EquipmentController.instance.UnEquip(this);
    }
}
public enum EquipTypes
{
    Helmet, Armor, Shield, Weapon, Shoe
}