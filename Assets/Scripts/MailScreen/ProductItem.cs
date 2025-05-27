using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Sprite removeSprite;
    [SerializeField] private Sprite addSprite;

    public string ProductName { get; private set; }
    public bool IsInMail { get; private set; }
    public Action<ProductItem> OnToggleClicked;

    public void Initialize(string displayName, string productName, bool isInMail)
    {
        ProductName = productName;
        IsInMail = isInMail;

        if (nameText != null)
            nameText.text = displayName;

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(OnToggleButtonClicked);
            SetButtonSprite(isInMail);
        }
    }

    private void OnToggleButtonClicked()
    {
        OnToggleClicked?.Invoke(this);
    }

    public void SetButtonSprite(bool isInMail)
    {
        var image = toggleButton?.GetComponent<Image>();
        if (image != null)
            image.sprite = isInMail ? removeSprite : addSprite;
    }
}
