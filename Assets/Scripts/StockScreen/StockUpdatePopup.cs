using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockUpdatePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text prevText;
    [SerializeField] private TMP_Text prevTextHeader;
    [SerializeField] private TMP_InputField newInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text productNameText;
    private string _productName;


    void OnEnable()
    {
        ResetUI();
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
    }

    void OnDisable()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    public void Initialize(string productName, string prevTextValue)
    {
        _productName = productName;

        if (productNameText != null)
        {
            productNameText.text = productName;
        }

        if (newInputField != null)
        {
            newInputField.text = string.Empty;
        }

        prevText.transform.parent.gameObject.SetActive(prevTextValue != "");
        
        if (prevText != null)
        {
            prevText.text = prevTextValue;
        }
    }

    private void OnSubmitButtonClicked()
    {

        if (string.IsNullOrWhiteSpace(newInputField.text))
        {
            GUIManager.Instance.ShowAndroidToast("Please enter a valid value.");
            return;
        }
        StockScreen.StockUpdate(_productName, newInputField.text);
        GUIManager.Instance.ShowAndroidToast("Updated successfully!");
        Close();

        StockScreen.CallUpdateSubmitted();

    }

    private void ResetUI()
    {
        if (prevText != null)
        {
            prevText.text = string.Empty;
        }
    }
    
    public void Close()
    {
        ResetUI();
        GetComponent<PopupAnimator>()?.Hide();
    }
}
