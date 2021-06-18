using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private CharacterStats character;
    [SerializeField] private Image healthImage;

    void Start()
    {
        character.OnHealthValueChanged += HandleHealthValueChanged;

        HandleHealthValueChanged();
    }

    private void HandleHealthValueChanged()
    {
        Debug.Log($"Health: {character.GetCurrentHealth().GetValue()}/{character.GetMaxHealth().GetValue()}");

        healthImage.fillAmount = character.GetCurrentHealth().GetValue() / character.GetMaxHealth().GetValue();
    }
}
