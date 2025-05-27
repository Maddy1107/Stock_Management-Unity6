using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ProductMail : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mailContent;
    public GameObject productContent;
    public GameObject productPrefab;
    public GameObject finalMailContent;
    public TMP_InputField searchInputField;
    public Button clearSearchButton, buildEmailButton;

    private List<string> productList;
    private HashSet<string> selectedProducts = new HashSet<string>();
    private Transform productParent;
    private Transform mailParent;
    private string headerText;
    public TMP_Text mailText;

    void OnEnable()
    {
        OpenScreen();
        productList = JsonUtilityReader.ReadProductJson("product_list.json");
        productParent = productContent.transform;
        mailParent = mailContent.transform;

        SetupUIListeners();
        GenerateAllProductLists();
    }

    private void SetupUIListeners()
    {
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.RemoveAllListeners();
            searchInputField.onValueChanged.AddListener(SearchProducts);
        }
        if (clearSearchButton != null)
        {
            clearSearchButton.onClick.RemoveAllListeners();
            clearSearchButton.onClick.AddListener(ClearSearch);
        }
        if (buildEmailButton != null)
        {
            buildEmailButton.onClick.RemoveAllListeners();
            buildEmailButton.onClick.AddListener(BuildEmailBody);
        }
    }

    public void OpenScreen()
    {
        gameObject.SetActive(true);
        finalMailContent.SetActive(false);
    }

    public void OpenWithType(MailType type)
    {
        gameObject.SetActive(true);
        headerText = $"Below are the {(type == MailType.Required ? "required" : "received")} products:";
        ResetProductScreen();
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

        foreach (var product in productList)
        {
            if (selectedProducts.Contains(product)) continue;
            if (!isSearch || product.ToLower().Contains(lowerSearch))
            {
                CreateProductItem(product, productParent, false);
                count++;
                if (!isSearch && count >= 50) return;
            }
        }
    }

    private void RefreshMailList()
    {
        ClearChildren(mailParent);

        int index = 1;
        foreach (string name in selectedProducts)
        {
            CreateProductItem(name, mailParent, true, index++);
        }
    }

    private void CreateProductItem(string name, Transform parent, bool isInMail, int displayIndex = -1)
    {
        GameObject itemGO = Instantiate(productPrefab, parent);
        ProductItem item = itemGO.GetComponent<ProductItem>();
        string displayName = (displayIndex > 0) ? $"{displayIndex}. {name}" : name;
        item.Initialize(displayName, name, isInMail);
        item.OnToggleClicked += HandleToggleClicked;
    }

    private void HandleToggleClicked(ProductItem item)
    {
        string name = item.ProductName;
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
            RemoveProductFromList(name);
        }
    }

    private void RemoveProductFromList(string name)
    {
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

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;
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

    public void BuildEmailBody()
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(headerText))
        {
            sb.AppendLine(headerText);
            sb.AppendLine();
        }

        int index = 1;
        foreach (var name in selectedProducts)
        {
            sb.AppendLine($"{index++}. {name}");
        }

        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString(), mailText);
        gameObject.SetActive(false);
        finalMailContent.SetActive(true);
    }

    public void ResetProductScreen()
    {
        selectedProducts.Clear();
        ClearSearch();
        RefreshMailList();
        finalMailContent.SetActive(false);
    }
}
