using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BaseItem", menuName = "Items")]
public class Item : ScriptableObject
{
    // Data
    public int ItemId = -1;
    public string ItemName = "InValid";
    [TextArea]
    public string Description = "This is an invalid item";
    public bool bCanTake = false;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Implement custom behaviour when the item is used to reap its benefits
    virtual public void UseItem()
    {

    }

     // Implement custom behaviour when the item is exchanged to cook a higher level item
    virtual public void ExchangeItem()
    {

    }
}
