using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalListItem : MonoBehaviour
{
    [SerializeField] private Button productNameText;
    [SerializeField] private TMP_Text productValueText;

    public string ProductName { get; private set; }
    public string ProductValue { get; private set; }

    public void SetData(string productName, string productValue)
    {
        ProductName = productName;
        ProductValue = productValue;

        if (productNameText != null)
        {
            productNameText.gameObject.GetComponentInChildren<TMP_Text>().text = productName;
        }

        if (productValueText != null)
        {
            productValueText.text = productValue;
        }

        productNameText.onClick.RemoveAllListeners();
        productNameText.onClick.AddListener(HandleToggleClicked);
    }

    private void HandleToggleClicked()
    {
        GameEvents.InvokeOnEditToggleClicked(this);
    }
}
