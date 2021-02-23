using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
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

    // Properties
    public float CarryCapacity = 10;
    List<ItemData> InventoryList = new List<ItemData>();
    List<ItemData> EquippedList = new List<ItemData>();
    public LayerMask ItemMask;

    // PlayerProperties
    FirstPersonAIO FPCharacter;

    // UI properties
    public GameObject InventoryPanel;
    Transform InventoryItemPanel;
    Transform InventoryEquipPanel;
    TMPro.TMP_Text InventoryStatusTextField;
    public GameObject ItemSlotPrefab;
    public int MaxNumSlots = 12;

    // properties from PlayerActionsScript
    KeyCode InteractButton;
    float InteractionRange;
    Camera PlayerCamera;

    // Start is called before the first frame update
    void Start()
    {
        // Get required properties from player actions
        PlayerActions PA = GetComponent<PlayerActions>();
        InteractButton = PA.InteractButton;
        InteractionRange = PA.InteractionRange;
        PlayerCamera = PA.PlayerCamera;

        FPCharacter = GetComponent<FirstPersonAIO>();

        InventoryItemPanel = InventoryPanel.transform.GetChild(1);
        InventoryStatusTextField = InventoryPanel.transform.GetChild(2).GetComponent<TMPro.TMP_Text>();
        InventoryEquipPanel = InventoryPanel.transform.GetChild(4);

        // Initialise slots
        InventoryList.Clear();
        for (int i = 0; i < MaxNumSlots; i++)
        {
            InventoryList.Add(new ItemData(null, 0));
        }
        EquippedList.Clear();
        for (int i = 0; i < InventoryEquipPanel.transform.childCount; i++)
        {
            EquippedList.Add(new ItemData(null, 0));
        }

        RefreshInventoryUI();
        InventoryPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckForItemPickups();
        ManageInventoryUI();
    }

    void ClearItemButtons()
    {
        for(int i=0; i< InventoryItemPanel.childCount; i++)
        {
            GameObject.Destroy(InventoryItemPanel.GetChild(i).gameObject);
        }
    }

    void RefreshInventoryUI()
    {
        ClearItemButtons();
        foreach(ItemData i in InventoryList)
        {
            GameObject tempSlot = Instantiate<GameObject>(ItemSlotPrefab);
            GameObject tempItem = tempSlot.transform.GetChild(0).gameObject;
            tempItem.GetComponent<ItemButtonHandler>().Init(i, this);
            tempSlot.transform.SetParent(InventoryItemPanel);
        }
        InventoryStatusTextField.text = "Carrying : " + GetInventoryWeight() + ", Capacity : " + CarryCapacity.ToString();
        //Debug.Log(InventoryEquipPanel.transform.childCount);
        // Refresh the equip panel
        for (int i=0; i < InventoryEquipPanel.transform.childCount; i++)
        {
            InventoryEquipPanel.GetChild(i).GetChild(0).GetComponent<ItemButtonHandler>().Init(EquippedList[i], this);
        }

    }

    void ManageInventoryUI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (InventoryPanel.activeSelf){
                InventoryPanel.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                FPCharacter.playerCanMove = true;
                FPCharacter.enableCameraMovement = true;
            }
            else
            {
                InventoryPanel.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                FPCharacter.playerCanMove = false;
                FPCharacter.enableCameraMovement = false;
            }
        }
    }

    void CheckForItemPickups()
    {
        if (Input.GetKeyDown(InteractButton))
        {
            RaycastHit hit;
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, InteractionRange, ItemMask))
            {
                ItemPickup PickUp = hit.transform.GetComponentInParent<ItemPickup>();
                if (PickUp == null)
                    PickUp = hit.transform.GetComponentInChildren<ItemPickup>();
                if (PickUp == null)
                {
                    return;
                }
                PickUpItem(PickUp);
                return;
            }
        }
    }

    void PickUpItem(ItemPickup PickedItem)
    {
        if (CanTakeItem(PickedItem.Item, PickedItem.Count))
        {
            AddItem(PickedItem);
            PickedItem.OnPickedUp();
        }
    }

    bool CanTakeItem(Item Item, int Count)
    {
        return GetInventoryWeight()+Item.Weight*Count <= CarryCapacity && GetNumEmptySlots() > 0;
    }

    float GetInventoryWeight()
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

    int GetNumEmptySlots()
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

    void AddItem(ItemPickup PickedItem)
    {
        AddItem(new ItemData(PickedItem));
    }

    void AddItem(ItemData Itemdata)
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

    void AddToFreeSlot(ItemData ItemData)
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

    // Drops the item from the inventory and generates a pickup in the world to represent it.
    public void DropItem(Item Item, int Count = 1)
    {
        // If the item is not droppable, do nothing
        if (!ItemManager.instance.GetItemFromId(Item.ItemId).bCanDrop) return;

        for (int i = 0; i < InventoryList.Count; i++)
        {
            if (InventoryList[i].Item == Item)
            {
                // Remove the item from the inventory
                ItemData temp = InventoryList[i];
                int DropCount = System.Math.Min(temp.Count, Count);
                temp.Count -= DropCount;
                InventoryList[i] = temp;

                // Refresh item inventory and refresh UI
                if (InventoryList[i].Count == 0)
                {
                    InventoryList[i] = new ItemData(null, 0);
                }
                RefreshInventoryUI();

                // Spawn the removed item into the world as a pickup
                ItemManager.instance.GenerateItemFromId(Item.ItemId, Camera.main.transform.position+ Camera.main.transform.forward * 1, new Quaternion() , DropCount);
                return;
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
        Debug.Log("Switching");
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
            InventoryList[FindItemSlot(Itemdata.Item)] = new ItemData(null, 0);
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
            EquippedList[EquipIndex] = new ItemData(null, 0);

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

    public void UseItem(ItemData Itemdata)
    {
        Itemdata.Item.OnItemUsed();
        Debug.Log("ItemUsed");
    }

    void CraftItem(ItemData ItemToCraft)
    {

    }
}
