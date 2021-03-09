using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    public int level;
    public int health;
    public Vector3 position;

    public GameObject GameManager;
    void Start()
    {
        EquipmentController.instance.onEquipmentChanged += OnEquipmentChanged;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(null, 30f);
        }
    }

    void OnEquipmentChanged(EquipableItem newItem, EquipableItem oldItem)
    {
        if (newItem != null)
        {
            Armor.AddModifier(newItem.armorModifier);
            Damage.AddModifier(newItem.damageModifier);

            if (newItem.weapon != null)
                weapon = newItem.weapon;
        }

        if (oldItem != null)
        {
            Armor.RemoveModifier(oldItem.armorModifier);
            Damage.RemoveModifier(oldItem.damageModifier);
            weapon = null;
        }
    }
    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
        GameManager.GetComponent<PauseMenu>().PopulateSavedGames();
    }
    public void LoadPlayer(PlayerData data)
    {

        level = data.level;
        health = data.health;

        position.x = data.positon[0];
        position.y = data.positon[1];
        position.z = data.positon[2];

    }
}
