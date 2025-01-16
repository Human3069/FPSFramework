using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Sellable : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI titleText;
    [SerializeField]
    protected TextMeshProUGUI priceText;

    public void Set(string title, int price)
    {
        titleText.text = title;
        priceText.text = price.ToString() + " Gold";
    }
}
