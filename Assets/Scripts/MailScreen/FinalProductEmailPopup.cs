using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalProductEmailPopup : Popup<FinalProductEmailPopup>
{
    [Header("UI References")]
    [SerializeField] private TMP_Text emailheadText;
    [SerializeField] private TMP_Text emailfootText;
    [SerializeField] private Button copyButton;
    [SerializeField] private GameObject productsTextPrefab;
    [SerializeField] private GameObject productsTextContainer;

    private string subjectText;
    private string emailBodyText;

    public void OnEnable()
    {
        copyButton.onClick.AddListener(() => GUIManager.Instance?.CopyToClipboard($"{subjectText}\n\n{emailBodyText}"));
    }

    public void OnDisable()
    {
        copyButton.onClick.RemoveAllListeners();
    }

    public void SetEmailContent(List<string> productList, string headerText, string subject = "")
    {
        string emailContent = ComposeEmailContent(productList);

        emailheadText.text = $"Dear Team,\n\n{headerText}\n";
        GenerateProductsToView(productList);
        emailfootText.text = "Thank you\nPriyanka Roy";
        subjectText = subject;

        BuildFullEmail(emailContent);
    }

    private void GenerateProductsToView(List<string> productList)
    {
        if (productList == null || productList.Count == 0)
        {
            Debug.LogWarning("No products to display.");
            return;
        }
        
        // Clear previous product texts
        ClearChildren(productsTextContainer.transform);

        int index = 1;
        foreach (var name in productList)
        {
            var productText = Instantiate(productsTextPrefab);
            productText.transform.SetParent(productsTextContainer.transform, false);
            productText.GetComponent<TMP_Text>().text = $"    {index++}. {name}";
            productText.SetActive(true);
        }
    }

    private string ComposeEmailContent(List<string> productList)
    {
        StringBuilder sb = new StringBuilder();

        int index = 1;
        foreach (var name in productList)
        {
            sb.AppendLine($"    {index++}. {name}");
        }

        return sb.ToString();
    }

    public void BuildFullEmail(string emailContent)
    {
        emailBodyText = string.Empty;

        var sb = new StringBuilder()
            .AppendLine(emailheadText.text)
            .AppendLine()
            .AppendLine(emailContent)
            .AppendLine()
            .AppendLine(emailfootText.text);

        emailBodyText = sb.ToString();

        Debug.Log($"Email body built:\n{emailBodyText}");
        Debug.Log($"Email subject: {subjectText}");
    }
    
    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

}
