using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
public class HealthBar : MonoBehaviour
{
    public CharacterStats Character;
    public Slider Health;

    void Start()
    {
        Character.OnHealthValueChange += Character_OnHealthValueChange;
        Health.maxValue = Character.maxHealth.GetValue();    
        Health.value = Character.currentHealth.GetValue();
    }

    private void Character_OnHealthValueChange(object sender, System.EventArgs e)
    {
        hp_value = Character.currentHealth.GetValue();
    }

    public float hp_value
    {
        get => Health.value;
        set => Health.value = value > Health.maxValue ? Health.value = Health.maxValue : Health.value = value;
    }
}*/
