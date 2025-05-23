using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MailScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mailContent;
    public GameObject productContent;
    public GameObject productPrefab;
    public TMP_InputField searchInputField;

    private Dictionary<string, List<string>> productList;
    private HashSet<string> selectedProducts = new HashSet<string>();

    private Transform productParent;
    private Transform mailParent;

    public Button clearSearchButton, copyButton, backButton;

    public TMP_Text titleText, headerText, footerText;

    void OnEnable()
    {
        productList = JsonUtilityReader.ReadCategoryJson("closing_stock_categories.json");
        productParent = productContent.transform;
        mailParent = mailContent.transform;

        if (searchInputField != null)
        {
            searchInputField.onValueChanged.RemoveAllListeners();
            searchInputField.onValueChanged.AddListener(SearchProducts);
            clearSearchButton.onClick.RemoveAllListeners();
            clearSearchButton.onClick.AddListener(ClearSearch);
        }

        if (copyButton != null)
        {
            copyButton.onClick.RemoveAllListeners();
            copyButton.onClick.AddListener(CopyToClipboard);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                GUIManager.Instance.ShowMainMenuPanel();
            });
        }
        GenerateAllProductLists();
    }


    public void OpenWithType(string type)
    {
        gameObject.SetActive(true);
        headerText.text = $"Below are the {(type == "required" ? "required" : "received")} products:";
    }

    void CopyToClipboard()
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(titleText.text))
        {
            sb.AppendLine(titleText.text);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(headerText.text))
        {
            sb.AppendLine(headerText.text);
            sb.AppendLine();
        }

        int index = 1;
        foreach (var name in selectedProducts)
        {
            sb.AppendLine($"    {index}. {name}");
            sb.AppendLine();
            index++;
        }

        if (!string.IsNullOrWhiteSpace(footerText.text))
        {
            sb.AppendLine(footerText.text);
            sb.AppendLine();
        }

        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log($"Copied to clipboard:\n{sb}");
    }

    public void GenerateAllProductLists()
    {
        PopulateProductList(null);
        RefreshMailList();
    }

    public void SearchProducts(string searchText)
    {
        PopulateProductList(string.IsNullOrWhiteSpace(searchText) ? null : searchText);
    }

    private void PopulateProductList(string searchText)
    {
        ClearChildren(productParent);

        if (productList == null) return;

        bool isSearch = !string.IsNullOrWhiteSpace(searchText);
        string lowerSearch = isSearch ? searchText.ToLower() : "";
        int count = 0;

        foreach (var category in productList)
        {
            foreach (string productName in category.Value)
            {
                if (selectedProducts.Contains(productName)) continue;

                if (!isSearch || productName.ToLower().Contains(lowerSearch))
                {
                    CreateProductItem(productName, productParent, false);
                    count++;
                    if (!isSearch && count >= 50) return;
                }
            }
        }
    }

    private void RefreshMailList()
    {
        ClearChildren(mailParent);

        int index = 1;
        foreach (string name in selectedProducts)
        {
            CreateProductItem($"{index}. {name}", mailParent, true);
            index++;
        }
    }

    private void CreateProductItem(string name, Transform parent, bool isInMail)
    {
        GameObject itemGO = Instantiate(productPrefab, parent);
        ProductItem item = itemGO.GetComponent<ProductItem>();

        item.Initialize(name, isInMail);
        item.OnToggleClicked += HandleToggleClicked;
    }

    private void HandleToggleClicked(ProductItem item)
    {
        string rawName = item.ProductName;

        // If mail item has a prefix like "1. Apple", extract actual name
        string name = rawName.Contains(". ") ? rawName.Substring(rawName.IndexOf(". ") + 2) : rawName;

        item.OnToggleClicked -= HandleToggleClicked;

        if (item.IsInMail)
        {
            selectedProducts.Remove(name);
            Destroy(item.gameObject);
            RefreshMailList();
            CreateProductItem(name, productParent, false);
        }
        else
        {
            selectedProducts.Add(name);
            Destroy(item.gameObject);
            CreateProductItem(name, mailParent, true);

            // Remove from product list view (only the one visible item)
            for (int i = 0; i < productParent.childCount; i++)
            {
                ProductItem p = productParent.GetChild(i).GetComponent<ProductItem>();
                if (p != null && p.ProductName == name)
                {
                    Destroy(p.gameObject);
                    break;
                }
            }
        }
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void ClearSearch()
    {
        if (searchInputField != null)
        {
            searchInputField.text = "";
            PopulateProductList(null);
        }
    }

}
