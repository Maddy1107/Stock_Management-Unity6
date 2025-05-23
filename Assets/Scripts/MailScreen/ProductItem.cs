using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductItem : MonoBehaviour
{
    public TMP_Text nameText;
    public Button toggleButton;

    public string ProductName { get; private set; }
    public bool IsInMail { get; private set; }
    public Sprite removeSprite;
    public Sprite addSprite;
    public Action<ProductItem> OnToggleClicked; // Changed from event

    public void Initialize(string productName, bool isInMail)
    {
        ProductName = productName;
        IsInMail = isInMail;

        if (nameText != null)
            nameText.text = productName;

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(() =>
            {
                OnToggleClicked?.Invoke(this);
            });

            toggleButton.GetComponentInChildren<TMP_Text>().text = isInMail ? "-" : "+";
            toggleButton.GetComponent<Image>().sprite = isInMail ? removeSprite : addSprite;

        }
    }
}
