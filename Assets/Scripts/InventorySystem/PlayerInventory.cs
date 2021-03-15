using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : Inventory
{
    // Properties
    public float CarryCapacity = 10;
    public LayerMask ItemMask;

    // PlayerProperties
    FirstPersonAIO FPCharacter;

    // properties from PlayerActionsScript
    KeyCode InteractButton;
    float InteractionRange;
    Camera PlayerCamera;

    private Transform InventoryEquipPanel;
    public bool ShopAccessed;
    public MerchantShop shop;

    public override void Start()
    {
        base.Start();
        // Get required properties from player actions
        PlayerActions PA = GetComponent<PlayerActions>();
        InteractButton = PA.InteractButton;
        InteractionRange = PA.InteractionRange;
        PlayerCamera = PA.PlayerCamera;

        FPCharacter = GetComponent<FirstPersonAIO>();

        InventoryEquipPanel = InventoryPanel.transform.GetChild(4);
        EquippedList.Clear();
        for (int i = 0; i < InventoryEquipPanel.transform.childCount; i++)
        {
            EquippedList.Add(EmptyItemData);
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

    public void UseOrSell(ItemData Itemdata)
    {
        if (ShopAccessed)
        {
            RemoveItem(new ItemData(Itemdata.Item, 1));
            shop.Sell(Itemdata);

            RefreshInventoryUI();
        }
        else
        {
            Itemdata.Item.OnItemUsed();
        }
    }

    void ManageInventoryUI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (InventoryPanel.activeSelf)
            {
                if (!ShopAccessed)
                {
                    InventoryPanel.SetActive(false);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    FPCharacter.playerCanMove = true;
                    FPCharacter.enableCameraMovement = true;
                }
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
                    InventoryList[i] = EmptyItemData;
                }
                RefreshInventoryUI();

                // Spawn the removed item into the world as a pickup
                ItemManager.instance.GenerateItemFromId(Item.ItemId, Camera.main.transform.position + Camera.main.transform.forward * 1, new Quaternion(), DropCount);
                return;
            }
        }
    }

    public override void RefreshInventoryUI()
    {
        ClearItemButtons();
        foreach (ItemData i in InventoryList)
        {
            if (i.Count != 0)
            {
                GameObject tempSlot = Instantiate<GameObject>(ItemSlotPrefab);
                GameObject tempItem = tempSlot.transform.GetChild(0).gameObject;
                tempItem.GetComponent<ItemButtonHandler>().Init(i, this);
                tempSlot.transform.SetParent(InventoryItemPanel);
            }
        }
        //Debug.Log(InventoryEquipPanel.transform.childCount);
        // Refresh the equip panel
        for (int i = 0; i < InventoryEquipPanel.transform.childCount; i++)
        {
            InventoryEquipPanel.GetChild(i).GetChild(0).GetComponent<ItemButtonHandler>().Init(EquippedList[i], this);
        }
        InventoryStatusTextField.text = "Carrying : " + GetInventoryWeight() + ", Capacity : " + CarryCapacity.ToString();
    }

    bool CanTakeItem(Item Item, int Count)
    {
        return GetInventoryWeight() + Item.Weight * Count <= CarryCapacity && GetNumEmptySlots() > 0;
    }

    void PickUpItem(ItemPickup PickedItem)
    {
        if (CanTakeItem(PickedItem.Item, PickedItem.Count))
        {
            AddItem(PickedItem);
            PickedItem.OnPickedUp();
        }
    }
}
