using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MerchantShop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<Inventory.ItemData> Items = new List<Inventory.ItemData>();

    public GameObject Player;
    public GameObject ShopPanel;
    public GameObject ItemSlotPrefab;
    public GameObject SlotPanel;

    public EventTrigger a;
    bool IsInteracted;

    float PlayerOriginalMouseSensitivity;
    float PlayerOriginalMouseSensitivityInternal;

    FirstPersonAIO firstPerson;
    TextMeshProUGUI InfoText;

    void Start()
    {
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
                IsInteracted = false;
                ShopPanel.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Player.GetComponent<Inventory>().InventoryPanel.SetActive(false);
                FreezeCamera(false);
            }
        }
    }

    public IEnumerator Interact()
    {
        ShopPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        IsInteracted = true;

        FreezeCamera(true);


        for (int i = 0; i < SlotPanel.transform.childCount; i++)
            Destroy(SlotPanel.transform.GetChild(i).gameObject);
        

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < Items.Count; i++)
        {
            GameObject Slot = Instantiate(ItemSlotPrefab, SlotPanel.transform);
            Button SlotBtn = Slot.transform.GetChild(0).GetComponent<Button>();

            Slot.transform.name = "a";

            SlotBtn.name = i.ToString();
            SlotBtn.onClick.AddListener(() => Buy());
            
            
            Slot.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Items[i].Item.name;
            Slot.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Items[i].Count.ToString();
        }
    }

    public void Buy()
    {
        int ItemIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);

        int temp = Items[ItemIndex].Count;

        if (temp > 0 && PlayerProgress.Currency >= Items[ItemIndex].Value)
        {
            Player.GetComponent<Inventory>().AddItem(new Inventory.ItemData(Items[ItemIndex].Item, 1, Items[ItemIndex].Value));
            temp--;
            Items[ItemIndex] = new Inventory.ItemData(Items[ItemIndex].Item, temp, Items[ItemIndex].Value);
                     
            Transform Slot = EventSystem.current.currentSelectedGameObject.GetComponent<Transform>();
            Slot.GetChild(1).GetComponent<TextMeshProUGUI>().text = temp.ToString();
            PlayerProgress.Currency -= Items[ItemIndex].Value;
            InfoText.text = $"Credits: {PlayerProgress.Currency}  Cost: {Items[ItemIndex].Value}";
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

    public void GetButtonNameByHover(Button button)
    {
       // int ItemIndex = int.Parse(button.name);
        //Debug.Log($"{button.name}  Credits: {PlayerProgress.Currency}  Cost: {Items[ItemIndex].Value}");
        //InfoText.text = $"asdasd";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Triggered");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
}
