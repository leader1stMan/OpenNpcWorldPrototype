using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AmountField : MonoBehaviour
{
    public int Amount { get; private set; }

    private InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<InputField>();
    }

    private void Update()
    {
        int amount;

        if (int.TryParse(inputField.text, out amount))
        {
            Amount = amount;
        }
        else
        {
            Amount = 0;
            inputField.text = "0";
        }
    }
}
