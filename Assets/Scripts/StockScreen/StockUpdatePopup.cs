using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockUpdatePopup : UIPopup<StockUpdatePopup>
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField bottleInputField;
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

    public void Initialize(string productName)
    {
        _productName = productName;
        productNameText.text = productName;
        newInputField.text = string.Empty;
        bottleInputField.text = string.Empty;
    }

    public void Show(string productName)
    {
        Initialize(productName);
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
        string valueInput = newInputField.text.Trim();
        string bottleInput = bottleInputField.text.Trim();

        if (string.IsNullOrEmpty(valueInput))
        {
            GUIManager.Instance.ShowAndroidToast("Please enter a valid value.");
            return;
        }

        StockScreen.Instance.UpdateStock(_productName, valueInput, bottleInput);
        GUIManager.Instance.ShowAndroidToast("Updated successfully!");
        GameEvents.InvokeOnUpdateSubmitted(_productName);
        GameEvents.InvokeOnEditToggleClicked();
        Hide();
    }

    private void ResetUI()
    {
        newInputField.text = string.Empty;
        productNameText.text = string.Empty;
    }
}
