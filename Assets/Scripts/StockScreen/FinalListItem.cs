using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalListItem : MonoBehaviour
{
    [SerializeField] private Button productNameButton;
    [SerializeField] private TMP_Text productNameLabel;
    [SerializeField] private TMP_Text productValueText;

    public string ProductName { get; private set; }
    public string ProductValue { get; private set; }

    private void Awake()
    {
        if (productNameButton != null)
        {
            productNameButton.onClick.AddListener(OnProductNameClicked);
        }
    }

    private void OnDestroy()
    {
        if (productNameButton != null)
        {
            productNameButton.onClick.RemoveListener(OnProductNameClicked);
        }
    }

    public void SetData(string productName, string productValue)
    {
        ProductName = productName;
        ProductValue = productValue;

        if (productNameLabel != null)
        {
            productNameLabel.text = productName;
        }

        if (productValueText != null)
        {
            productValueText.text = productValue;
        }
    }

    private void OnProductNameClicked()
    {
        GameEvents.InvokeOnEditToggleClicked(this);
    }
}
