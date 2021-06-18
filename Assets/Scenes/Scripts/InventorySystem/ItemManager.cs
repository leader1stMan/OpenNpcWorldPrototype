using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    static public ItemManager instance = null;
    public List<Item> ItemDB = new List<Item>()
    {

    };

    Item InvalidItem;
    public GameObject ItemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        instance = this;
    }

    public Item GetItemFromId(int ItemId)
    {
        Item Item;
        foreach(Item i in ItemDB)
        {
            if(i.ItemId == ItemId)
            {
                Item = i;
                return Item;
            }
        }
        return InvalidItem;
    }

    public GameObject GenerateItemFromId(int ItemId, Vector3 Position, Quaternion Rotation, int Count = 1)
    {
        if(Count < 1)           // Can't generate less than 1 items
        {
            return null;
        }

        Item item = GetItemFromId(ItemId);

        RaycastHit Hit;
        Physics.Raycast(Position, new Vector3(0, -1, 0), out Hit);

        GameObject tempItem = Instantiate<GameObject>(ItemPrefab, Hit.point, Rotation);
        ItemPickup TempBase = tempItem.GetComponent<ItemPickup>();
        TempBase.Item = item;
        TempBase.Count = Count;
        return tempItem;
    }
}
