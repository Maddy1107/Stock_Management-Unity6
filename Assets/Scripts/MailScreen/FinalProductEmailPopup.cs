using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalProductEmailPopup : UIPopup<FinalProductEmailPopup>
{
    [Header("UI References")]
    [SerializeField] private TMP_Text emailHeadText;
    [SerializeField] private TMP_Text emailFootText;
    [SerializeField] private Button copyButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private GameObject productTextPrefab;
    [SerializeField] private Transform productTextContainer;

    private string subjectText;
    private string emailBodyText;
    private List<string> productList;

    private void OnEnable()
    {
        copyButton.onClick.AddListener(CopyToClipboard);
        saveButton.onClick.AddListener(SaveData);
    }

    private void OnDisable()
    {
        copyButton.onClick.RemoveAllListeners();
        saveButton.onClick.RemoveAllListeners();
    }

    public void SetEmailContent(List<string> products, string header, bool showSaveButton = true, string subject = "")
    {
        saveButton.gameObject.SetActive(showSaveButton);
        productList = products;
        subjectText = subject;

        // Setup header and footer
        emailHeadText.text = $"Dear Team,\n\n{header}\n";
        emailFootText.text = $"Thank you\n{PlayerPrefs.GetString("SavedUserName", "")}";

        GenerateProductList();
        emailBodyText = BuildEmailBody();

        Debug.Log($"Email subject: {subjectText}");
        Debug.Log($"Email body:\n{emailBodyText}");
    }

    private void GenerateProductList()
    {
        ClearChildren(productTextContainer);

        for (int i = 0; i < productList.Count; i++)
        {
            GameObject productItem = Instantiate(productTextPrefab, productTextContainer);
            if (productItem.TryGetComponent(out TMP_Text text))
                text.text = $"    {i + 1}. {productList[i]}";

            productItem.SetActive(true);
        }
    }

    private string BuildEmailBody()
    {
        var sb = new StringBuilder();
        sb.AppendLine(emailHeadText.text);

        for (int i = 0; i < productList.Count; i++)
            sb.AppendLine($"    {i + 1}. {productList[i]}");

        sb.AppendLine().AppendLine(emailFootText.text);
        return sb.ToString();
    }

    private void CopyToClipboard()
    {
        GUIManager.Instance?.CopyToClipboard($"{subjectText}\n\n{emailBodyText}");
    }

    private void SaveData()
    {
        LoadingScreen.Instance.Show();

        DBAPI.Instance.RequestProducts(productList,
            onSuccess: () =>
            {
                LoadingScreen.Instance.Hide();
                GUIManager.Instance.ShowAndroidToast("Products saved successfully.");
                Debug.Log("Products saved successfully.");
            },
            onError: error =>
            {
                LoadingScreen.Instance.Hide();
                GUIManager.Instance.ShowAndroidToast($"Error saving products: {error}");
                Debug.LogError($"Error saving products: {error}");
            });
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }
}
