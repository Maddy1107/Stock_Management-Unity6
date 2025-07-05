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

    public void SetEmailContent(List<string> productList, string header, string subject = "")
    {
        this.productList = productList;
        subjectText = subject;

        // Set up header/foot
        emailHeadText.text = $"Dear Team,\n\n{header}\n";
        emailFootText.text = $"Thank you\n{PlayerPrefs.GetString("SavedUserName", null)}";

        // Fill in product content
        GenerateProductList(productList);

        // Compose full email body
        emailBodyText = ComposeFullEmail(productList);
        Debug.Log($"Email body built:\n{emailBodyText}");
        Debug.Log($"Email subject: {subjectText}");
    }

    private void CopyToClipboard()
    {
        var fullText = $"{subjectText}\n\n{emailBodyText}";
        GUIManager.Instance?.CopyToClipboard(fullText);
    }

    private void GenerateProductList(List<string> products)
    {
        ClearChildren(productTextContainer);

        if (products == null || products.Count == 0)
        {
            Debug.LogWarning("No products to display.");
            return;
        }

        for (int i = 0; i < products.Count; i++)
        {
            var item = Instantiate(productTextPrefab, productTextContainer);
            if (item.TryGetComponent(out TMP_Text text))
            {
                text.text = $"    {i + 1}. {products[i]}";
            }
            item.SetActive(true);
        }
    }

    private string ComposeFullEmail(List<string> products)
    {
        var sb = new StringBuilder();
        sb.AppendLine(emailHeadText.text).AppendLine();

        for (int i = 0; i < products.Count; i++)
        {
            sb.AppendLine($"    {i + 1}. {products[i]}");
        }

        sb.AppendLine().AppendLine(emailFootText.text);
        return sb.ToString();
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void SaveData()
    {
        LoadingScreen.Instance.Show();

        DBAPI.Instance.RequestProducts(productList, () =>
        {
            LoadingScreen.Instance.Hide();
            Debug.Log("Products saved successfully.");
            GUIManager.Instance.ShowAndroidToast("Products saved successfully.");
        },
        (error) =>
        {
            LoadingScreen.Instance.Hide();
            Debug.LogError($"Error saving products: {error}");
            GUIManager.Instance.ShowAndroidToast($"Error saving products: {error}");
        });
    }
}
