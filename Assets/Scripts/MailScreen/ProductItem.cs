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

    public event Action<ProductItem> OnToggleClicked;

    public void Initialize(string displayName, string productName, bool isInMail)
    {
        ProductName = productName;
        IsInMail = isInMail;

        if (nameText != null)
            nameText.text = displayName;

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(HandleToggleClicked);
            SetButtonSprite(isInMail);
        }
    }

    private void HandleToggleClicked()
    {
        OnToggleClicked?.Invoke(this);
    }

    public void SetButtonSprite(bool isInMail)
    {
        IsInMail = isInMail;

        if (toggleButton?.GetComponent<Image>() is Image image)
        {
            image.sprite = isInMail ? removeSprite : addSprite;
        }
    }
}
