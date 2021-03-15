using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantInventory : Inventory
{
    public override void Start()
    {
        base.Start();
        RefreshInventoryUI();
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
    }

    public void OpenInventory()
    {
        if (!InventoryPanel.activeSelf)
        {
            InventoryPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
