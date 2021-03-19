using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour
{
    protected List<ItemData> InventoryList = new List<ItemData>();
    protected List<ItemData> EquippedList = new List<ItemData>();

    // UI properties
    public GameObject InventoryPanel;
    protected Transform InventoryItemPanel;
    protected TMPro.TMP_Text InventoryStatusTextField;
    public GameObject ItemSlotPrefab;
    public int MaxNumSlots = 12;

    // Start is called before the first frame update
    public virtual void Start()
    {
        InventoryItemPanel = InventoryPanel.transform.GetChild(1);
        InventoryStatusTextField = InventoryPanel.transform.GetChild(2).GetComponent<TMPro.TMP_Text>();

        // Initialise slots
        InventoryList.Clear();
        for (int i = 0; i < MaxNumSlots; i++)
        {
            InventoryList.Add(EmptyItemData);
        }
    }

    protected void ClearItemButtons()
    {
        for(int i=0; i< InventoryItemPanel.childCount; i++)
        {
            GameObject.Destroy(InventoryItemPanel.GetChild(i).gameObject);
        }
    }

    public abstract void RefreshInventoryUI();

    public float GetInventoryWeight()
    {
        float CarryingMass = 0.0f;
        foreach (ItemData i in InventoryList)
        {
            if (i.Item) // If not an empty slot
            {
                CarryingMass += i.Item.Weight * i.Count;        // weight of item * count of that item is the total wieght of that slot
            }
        }
        foreach (ItemData i in EquippedList)
        {
            if (i.Item) // If not an empty slot
            {
                CarryingMass += i.Item.Weight * i.Count;        // weight of item * count of that item is the total wieght of that slot
            }
        }
        return CarryingMass;
    }

    protected int GetNumEmptySlots()
    {
        int NumEmptySlots = 0;
        for (int i = 0; i < InventoryList.Count; i++)
        {
            if (!InventoryList[i].Item)     // Found an empty slot
            {
                NumEmptySlots++;                                  // Item type exists in inventory. Add count to it and return.
            }
        }
        return NumEmptySlots;
    }

    public void AddItem(ItemPickup PickedItem)
    {
        AddItem(new ItemData(PickedItem));
    }

    public void AddItem(ItemData Itemdata)
    {
        for (int i = 0; i < InventoryList.Count; i++)
        {
            if (InventoryList[i].Item && InventoryList[i].Item.ItemId == Itemdata.Item.ItemId)
            {
                ItemData temp = InventoryList[i];
                temp.Count += Itemdata.Count;
                InventoryList[i] = temp;

                RefreshInventoryUI();
                return;                                     // Item type exists in inventory. Add count to it and return.
            }
        }
        // At this point the item type does not exist already in the inventory list. So let's add it to the list.
        AddToFreeSlot(Itemdata);

        RefreshInventoryUI();
    }

    public void RemoveItem(ItemData Itemdata)
    {
        for (int i = 0; i < InventoryList.Count; i++)
        {
            if (InventoryList[i].Item && InventoryList[i].Item.ItemId == Itemdata.Item.ItemId)
            {
                ItemData temp = InventoryList[i];
                temp.Count -= Itemdata.Count;
                if (temp.Count > 0)
                {
                    InventoryList[i] = temp;
                }
                else
                {
                    InventoryList[i] = EmptyItemData;
                }

                RefreshInventoryUI();
                return;                                     // Item type exists in inventory. Add count to it and return.
            }
        }
    }

    public void AddToFreeSlot(ItemData ItemData)
    {
        for (int i = 0; i < InventoryList.Count; i++)
        {
            if (!InventoryList[i].Item)     // Found an empty slot
            {
                InventoryList[i] = ItemData;

                RefreshInventoryUI();
                return;                                     // Item type exists in inventory. Add count to it and return.
            }
        }
    }

    public int FindItemSlot(Item Item)
    {
        for(int i = 0;  i < InventoryList.Count; i++)
        {
            if(InventoryList[i].Item == Item)
            {
                return i;
            }
        }
        return -1;
    }

    public void SwitchItems(Item Item1, Item Item2)
    {
        int Item1Index = FindItemSlot(Item1);
        int Item2Index = FindItemSlot(Item2);
        if(Item1Index == -1 || Item2Index == -1)
        {
            return;
        }

        SwitchItems(Item1Index, Item2Index);
    }

    public void SwitchItems(int Item1Index, int Item2Index)
    {
        ItemData Item1Data = InventoryList[Item1Index];
        ItemData Item2Data = InventoryList[Item2Index];
        InventoryList[Item2Index] = Item1Data;
        InventoryList[Item1Index] = Item2Data;

        RefreshInventoryUI();
    }

    public void EquipItem(ItemData Itemdata, int EquipIndex)
    {
        int EquipItemInventorySlot = FindItemSlot(Itemdata.Item);

        UnEquip(EquipIndex);        // UnEquip any old item in the slot

        if (Itemdata.Item)      // If item to equip is valid
        {
            EquippedList[EquipIndex] = Itemdata;
            InventoryList[FindItemSlot(Itemdata.Item)] = EmptyItemData;
            Itemdata.Item.OnItemEquipped();

            RefreshInventoryUI();
        }

    }

    public void UnEquip(int EquipIndex, int OptionalIndex = -1)
    {
        if (EquippedList[EquipIndex].Item)      // Is there a valid item in the slot?
        {
            EquippedList[EquipIndex].Item.OnItemUnEquipped();
            if (OptionalIndex < 0) AddItem(EquippedList[EquipIndex]);
            else
            {
                if (InventoryList[OptionalIndex].Item) AddItem(EquippedList[EquipIndex]);
                else InventoryList[OptionalIndex] = EquippedList[EquipIndex];
            }
            EquippedList[EquipIndex] = EmptyItemData;

            RefreshInventoryUI();
        }
    }

    public bool CanEquipItem(ItemData Itemdata, int EquipIndex)
    {
        /*switch (EquipIndex)
        {
            case 0:
                return Itemdata.Item is HelmetItem;
            case 1:
                return Itemdata.Item is ArmorItem;
            case 2:
                return Itemdata.Item is ShieldItem;
            case 3:
                return Itemdata.Item is WeaponItem;
            case 4:
                return Itemdata.Item is ShoeItem;
            default: break;
        }*/
        EquipableItem temp = (EquipableItem)Itemdata.Item;
        return (int)temp.EquipType == EquipIndex;
    }
    public ItemData EmptyItemData => new ItemData(null, 0);  
}


[System.Serializable]
public struct ItemData
{
    public Item Item;
    public int Count;

    public ItemData(Item Item, int Count)
    {
        this.Item = Item;
        this.Count = Count;
    }
    public ItemData(ItemPickup PickUp)
    {
        this.Item = PickUp.Item;
        this.Count = PickUp.Count;
    }
};
