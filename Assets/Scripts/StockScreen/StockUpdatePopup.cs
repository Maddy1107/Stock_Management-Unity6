using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockUpdatePopup : UIPopup<StockUpdatePopup>
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text prevText;
    [SerializeField] private TMP_Text prevTextHeader;
    [SerializeField] private TMP_InputField newInputField;
    [SerializeField] private TMP_Text productNameText;

    [Header("Buttons")]
    [SerializeField] private Button submitButton;

    private string _productName;

    private void OnEnable()
    {
        ResetUI();
        BindListeners();
    }

    private void OnDisable()
    {
        UnbindListeners();
    }

    public void Initialize(string productName, string previousQuantity = "")
    {
        _productName = productName;
        productNameText.text = productName;
        newInputField.text = string.Empty;

        bool hasPrevious = !string.IsNullOrWhiteSpace(previousQuantity);
        prevText.transform.parent.gameObject.SetActive(hasPrevious);
        prevText.text = hasPrevious ? previousQuantity : string.Empty;
    }

    public void Show(string productName, string previousQuantity = "")
    {
        Initialize(productName, previousQuantity);
        Show();
    }

    private void BindListeners()
    {
        submitButton.onClick.AddListener(HandleSubmitClicked);
    }

    private void UnbindListeners()
    {
        submitButton.onClick.RemoveListener(HandleSubmitClicked);
    }

    private void HandleSubmitClicked()
    {
        string input = newInputField.text.Trim();

        if (string.IsNullOrEmpty(input))
        {
            GUIManager.Instance.ShowAndroidToast("Please enter a valid value.");
            return;
        }

        StockScreen.Instance.UpdateStock(_productName, input);
        GUIManager.Instance.ShowAndroidToast("Updated successfully!");
        GameEvents.InvokeOnUpdateSubmitted();

        Hide();
    }

    private void ResetUI()
    {
        newInputField.text = string.Empty;
        productNameText.text = string.Empty;
    }
}
