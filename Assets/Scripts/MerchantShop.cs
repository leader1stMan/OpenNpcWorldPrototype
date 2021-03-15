using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class MerchantShop : MonoBehaviour
{
    public List<ItemData> Items = new List<ItemData>();

    public GameObject Player;
    public GameObject ShopPanel;
    public GameObject ItemSlotPrefab;
    public GameObject SlotPanel;
    public MerchantInventory merchantInventory;

    bool IsInteracted;

    float PlayerOriginalMouseSensitivity;
    float PlayerOriginalMouseSensitivityInternal;

    NavMeshAgent agent;
    FirstPersonAIO firstPerson;
    PlayerInventory inventory;
    public static TextMeshProUGUI InfoText;

    public float CostIncreasment = 100;

    void Start()
    {
        merchantInventory = GetComponent<MerchantInventory>();
        inventory = Player.GetComponent<PlayerInventory>();
        agent = GetComponent<NavMeshAgent>();
        firstPerson = Player.GetComponent<FirstPersonAIO>();

        PlayerOriginalMouseSensitivity = firstPerson.mouseSensitivity;
        PlayerOriginalMouseSensitivityInternal = firstPerson.mouseSensitivityInternal;

        InfoText = ShopPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        ShopPanel.SetActive(false);      
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInteracted)
        {
            if (Vector3.Distance(transform.position, Player.transform.position) > 3)
            {
                agent.isStopped = false;
                firstPerson.playerCanMove = true;
                IsInteracted = false;
                ShopPanel.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Player.GetComponent<PlayerInventory>().InventoryPanel.SetActive(false);
                Player.GetComponent<PlayerInventory>().ShopAccessed = false;
                FreezeCamera(false);

            }
        }
    }

    public IEnumerator Interact()
    {
        merchantInventory.OpenInventory();
        inventory.shop = this;
        agent.isStopped = true;
        firstPerson.playerCanMove = false;
        //ShopPanel.SetActive(true);
        //Cursor.lockState = CursorLockMode.Confined;
        IsInteracted = true;

        FreezeCamera(true);

        yield return new WaitForSeconds(0.3f);
        /*
        for (int i = 0; i < SlotPanel.transform.childCount; i++)
            Destroy(SlotPanel.transform.GetChild(i).gameObject);
        

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < Items.Count; i++)
        {
            GameObject Slot = Instantiate(ItemSlotPrefab, SlotPanel.transform);
            Button SlotBtn = Slot.transform.GetChild(0).GetComponent<Button>();

            EventTrigger trigger = SlotBtn.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { GetButtonNameByHover((PointerEventData)data); });
            trigger.triggers.Add(entry);

            Slot.transform.name = i.ToString();
            SlotBtn.name = i.ToString();
            SlotBtn.onClick.AddListener(() => Buy());
                      
            Slot.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Items[i].Item.name;
            Slot.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Items[i].Count.ToString();
        }

        InfoText.text = $"Credits: {PlayerProgress.Currency}";
        */
    }

    public void Buy()
    {
        int ItemIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);

        int temp = Items[ItemIndex].Count;

        if (temp > 0 && PlayerProgress.Currency >= Items[ItemIndex].Item.ItemValue * (100 + CostIncreasment) / 100)
        {
            Player.GetComponent<PlayerInventory>().AddItem(new ItemData(Items[ItemIndex].Item, 1));
            temp--;
            if (temp == 0)
            {
                Items.Remove(Items[ItemIndex]);
                Destroy(EventSystem.current.currentSelectedGameObject.GetComponent<Transform>());
                PlayerProgress.Currency -= Items[ItemIndex].Item.ItemValue * (100 + CostIncreasment) / 100;
                InfoText.text = $"Credits: {PlayerProgress.Currency}";
            }
            else
            {
                Items[ItemIndex] = new ItemData(Items[ItemIndex].Item, temp); 
                Transform Slot = EventSystem.current.currentSelectedGameObject.GetComponent<Transform>();
                Slot.GetChild(1).GetComponent<TextMeshProUGUI>().text = temp.ToString();
                PlayerProgress.Currency -= Items[ItemIndex].Item.ItemValue * (100 + CostIncreasment) / 100;
                InfoText.text = $"Credits: {PlayerProgress.Currency}  Cost: {Items[ItemIndex].Item.ItemValue * (100 + CostIncreasment) / 100}";
            }
        }
    }
    public void Sell(ItemData item)
    {
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

    public void GetButtonNameByHover(PointerEventData data)
    {
        Transform Slot = data.pointerCurrentRaycast.gameObject.transform.parent.parent;
        InfoText.text = $"Credits: {PlayerProgress.Currency}  Cost: {Items[int.Parse(Slot.GetChild(0).name)].Item.ItemValue}";
    }
}
