using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ProductItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Sprite addSprite;
    [SerializeField] private Sprite removeSprite;

    public string ProductName { get; private set; }

    private bool _isInMail;
    public bool IsInMail
    {
        get => _isInMail;
        set
        {
            _isInMail = value;
            UpdateToggleVisual();
        }
    }

    /// <summary>
    /// Initializes this item.
    /// </summary>
    public void Initialize(string displayName, string productName, bool isInMail = false)
    {
        ProductName = productName;
        nameText?.SetText(displayName);

        if (toggleButton == null)
            toggleButton = GetComponent<Button>();

        SetupToggleButton();

        IsInMail = isInMail; // auto-updates visuals
    }

    private void SetupToggleButton()
    {
        if (toggleButton == null) return;

        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(HandleToggleClicked);
    }

    private void HandleToggleClicked()
    {
        GameEvents.InvokeOnToggleClicked(this);
    }

    private void UpdateToggleVisual()
    {
        if (toggleButton?.GetComponent<Image>() is Image image)
        {
            image.sprite = IsInMail ? removeSprite : addSprite;
        }
    }
}
