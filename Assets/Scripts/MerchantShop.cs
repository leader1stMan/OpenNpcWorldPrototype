using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MerchantShop : MonoBehaviour
{
    public GameObject ShopPanel;
    public GameObject ItemSlotPrefab;
    public GameObject SlotPanel;
    public MerchantInventory merchantInventory;




    void Start()
    {
        merchantInventory = GetComponent<MerchantInventory>();



    }

    // Update is called once per frame
    void Update()
    {
        /*
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
        */
    }

    public IEnumerator Interact()
    {
        merchantInventory.OpenInventory();
        //ShopPanel.SetActive(true);
        //Cursor.lockState = CursorLockMode.Confined;

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
    }
    
    public void GetButtonNameByHover(PointerEventData data)
    {
        Transform Slot = data.pointerCurrentRaycast.gameObject.transform.parent.parent;
        //InfoText.text = $"Credits: {PlayerProgress.Currency}  Cost: {Items[int.Parse(Slot.GetChild(0).name)].Item.ItemValue}";
    }
}
