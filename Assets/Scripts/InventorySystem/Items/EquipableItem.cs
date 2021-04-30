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

    GameObject objectInScene;
    override public void OnItemUsed(){
        
    }

    override public void OnItemEquipped(GameObject owner)
    {
        EquipmentController.instance.Equip(this);
        objectInScene = ItemManager.instance.GenerateItemFromId(ItemId, new Vector3(), new Quaternion(), 1);
        objectInScene.transform.SetParent(owner.GetComponent<PlayerStats>().WeaponBoneR);
        owner.GetComponent<AnimationController>().ChangeAnimation(AnimationController.SWORD_EQUIP, AnimatorLayers.UP, true);
    }

    override public void OnItemUnEquipped()
    {
        EquipmentController.instance.UnEquip(this);
        Debug.Log("destroy " + objectInScene);
        Destroy(objectInScene);
    }
}
public enum EquipTypes
{
    Helmet, Armor, Shield, Weapon, Shoe
}