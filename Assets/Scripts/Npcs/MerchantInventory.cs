using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class MerchantInventory : Inventory, IInteractWindow, IDestructible
{
    public GameObject Player;
    NavMeshAgent agent;
    FirstPersonAIO firstPerson;
    PlayerInventory inventory;

    float PlayerOriginalMouseSensitivity;
    float PlayerOriginalMouseSensitivityInternal;

    public float CostIncreasment = 100;

    public static TextMeshProUGUI InfoText;
    public override void Start()
    {
        base.Start();

        Player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        firstPerson = Player.GetComponent<FirstPersonAIO>();
        inventory = Player.GetComponent<PlayerInventory>();

        InfoText = InventoryPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        PlayerOriginalMouseSensitivity = firstPerson.mouseSensitivity;
        PlayerOriginalMouseSensitivityInternal = firstPerson.mouseSensitivityInternal;

        RefreshInventoryUI();
        InventoryPanel.SetActive(false);
    }
    public override void RefreshInventoryUI()
    {
        ClearItemButtons();
        foreach (ItemData i in InventoryList)
        {
            GameObject tempSlot = Instantiate<GameObject>(ItemSlotPrefab);
            GameObject tempItem = tempSlot.transform.GetChild(0).gameObject;
            tempItem.GetComponent<ItemButtonHandler>().Type = InventoryType.Merchant;
            tempItem.GetComponent<ItemButtonHandler>().Init(i, this);
            tempSlot.transform.SetParent(InventoryItemPanel);
        }
    }

    public void OnOpen()
    {
        if (!InventoryPanel.activeSelf)
        {
            Vector3 rot = transform.eulerAngles;
            transform.LookAt(Player.transform);
            transform.eulerAngles = new Vector3(rot.x, transform.eulerAngles.y, rot.z);

            inventory.InventoryPanel.SetActive(true);
            inventory.ShopAccessed = true;

            FreezeCamera(true);

            inventory.shop = this;
            agent.isStopped = true;
            firstPerson.playerCanMove = false;
            InventoryPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            InfoText.text = $"Credits: {PlayerProgress.Currency}";
        }
    }
    public void OnClose()
    {
        if (InventoryPanel.activeSelf)
        {
            inventory.InventoryPanel.SetActive(false);
            inventory.ShopAccessed = false;

            FreezeCamera(false);

            inventory.shop = null;
            agent.isStopped = false;
            InventoryPanel.SetActive(false);
            firstPerson.playerCanMove = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void BuyItem(ItemData item)
    {
        if (PlayerProgress.Currency >= item.Item.ItemValue * (100 + CostIncreasment) / 100)
        {
            Player.GetComponent<PlayerInventory>().AddItem(new ItemData(item.Item, 1));
            RemoveItem(new ItemData(item.Item, 1));
            PlayerProgress.Currency -= item.Item.ItemValue * (100 + CostIncreasment) / 100;
            InfoText.text = $"Credits: {PlayerProgress.Currency}";
        }
    }

    void FreezeCamera(bool IsFrozen)
    {
        if (IsFrozen)
        {
            firstPerson.mouseSensitivity = 0;
            firstPerson.mouseSensitivityInternal = 0;
        }
        else
        {
            firstPerson.mouseSensitivity = PlayerOriginalMouseSensitivity;
            firstPerson.mouseSensitivityInternal = PlayerOriginalMouseSensitivityInternal;
        }
    }

    public void Sell(ItemData item)
    {
        /*
        int index = 0;
        foreach (ItemData i in Items)
        {
            if (i.Item == item.Item)
            {
                Items[index] = new ItemData(Items[index].Item, i.Count + 1);
                Transform Slot = EventSystem.current.currentSelectedGameObject.GetComponent<Transform>();
                Slot.GetChild(1).GetComponent<TextMeshProUGUI>().text = (i.Count + 1).ToString();

                PlayerProgress.Currency += item.Item.ItemValue;
                InfoText.text = $"Credits: {PlayerProgress.Currency}  Cost: {Items[index].Item.ItemValue}";
                return;
            }
            index++;
        }
        Items.Add(new ItemData(item.Item, 1));
        */
        PlayerProgress.Currency += item.Item.ItemValue;
        InfoText.text = $"Credits: {PlayerProgress.Currency}";
        AddItem(item);
    }

    public void OnDestruction(GameObject destroyer)
    {
        OnClose();
    }
}
