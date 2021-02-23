using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BaseItem", menuName = "Items/BaseItem")]
public class Item : ScriptableObject
{
    // Data
    public int ItemId = -1;
    public string ItemName = "InValid";
    [TextArea]
    public string Description = "This is an invalid item";
    public bool bCanTake = false;
    public bool bCanDrop = true;
    public float Weight = 0.0f;
    
    // Rendering
    public Mesh ItemMesh;
    public Material ItemMaterial;
    public Sprite ItemImage;

    public Item()
    {
        
    }

    public Item(Item i)
    {
        this.ItemId = i.ItemId;
        this.ItemName = i.ItemName;
        this.Description = i.Description;
        this.bCanTake = i.bCanTake;
        this.Weight = i.Weight;
        this.ItemMesh = i.ItemMesh;
        this.ItemMaterial = i.ItemMaterial;
        this.ItemImage = i.ItemImage;
    }

    // Implement custom behaviour when the item is used to reap its benefits
    virtual public void OnItemUsed()
    {

    }

    // Implement custom behaviour when the item is equipped
    virtual public void OnItemEquipped()
    {

    }

    // Implement custom behaviour when the item is unequipped
    virtual public void OnItemUnEquipped()
    {

    }

    // Implement custom behaviour when the item is exchanged to cook a higher level item
    virtual public void OnItemExchanged()
    {

    }

    // Implement custom behaviour when the item is dropped
    virtual public void OnItemDropped()
    {
        
    }
}
