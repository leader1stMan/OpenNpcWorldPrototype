using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    public static EquipmentController instance;

    public delegate void OnEquipmentChanged(EquipableItem oldItem, EquipableItem newItem);
    public OnEquipmentChanged onEquipmentChanged;

    EquipableItem[] currentEquipment;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
       int numberOfSlots = System.Enum.GetNames(typeof(EquipTypes)).Length;
        currentEquipment = new EquipableItem[numberOfSlots];
    }
    public void Equip(EquipableItem item)
    {
        int slotIndex = (int)item.EquipType;
        EquipableItem oldItem = null;

        if (currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
        }

        currentEquipment[slotIndex] = item;

        if (onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(item, oldItem);
        }
    }

    public void UnEquip(EquipableItem item)
    {
        int slotIndex = (int)item.EquipType;

        currentEquipment[slotIndex] = null;

        if (onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(null, item);
        }
    }
}

