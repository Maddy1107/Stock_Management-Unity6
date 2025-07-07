using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public abstract class ProductListbase : MonoBehaviour
{
    [Header("Common UI References")]
    [SerializeField] protected TMP_InputField searchInputField;
    [SerializeField] protected GameObject productPrefab;
    [SerializeField] protected GameObject productContent;
    [SerializeField] protected Button clearSearchButton;

    protected List<string> productList;
    protected readonly List<string> selectedProducts = new();

    protected virtual void OnEnable()
    {
        productList = JsonUtilityEditor.ReadJsonFromResources<List<string>>("product_list");
        SetupListeners();
        PopulateProductList(null);

        GameEvents.OnUpdateSubmitted += RemoveProduct;
    }

    protected virtual void OnDisable()
    {
        RemoveListeners();
        GameEvents.OnUpdateSubmitted -= RemoveProduct;

    }

    private void RemoveProduct(string product)
    {
        productList.Remove(product);
        PopulateProductList(null);
    }

    protected void SetupListeners()
    {
        if (searchInputField) searchInputField.onValueChanged.AddListener(SearchProducts);
        if (clearSearchButton) clearSearchButton.onClick.AddListener(ClearSearch);
    }

    protected void RemoveListeners()
    {
        if (searchInputField) searchInputField.onValueChanged.RemoveAllListeners();
        if (clearSearchButton) clearSearchButton.onClick.RemoveAllListeners();
    }

    protected void SearchProducts(string searchText)
    {
        PopulateProductList(string.IsNullOrWhiteSpace(searchText) ? null : searchText);
    }

    protected void ClearSearch()
    {
        if (searchInputField) searchInputField.text = "";
        PopulateProductList(null);
    }

    protected void ClearChildren(Transform parent)
    {
        if (!parent) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    protected virtual void PopulateProductList(string searchText)
    {
        ClearChildren(productContent.transform);
        if (productList == null) return;

        bool isSearching = !string.IsNullOrWhiteSpace(searchText);
        string lowerSearch = isSearching ? searchText.ToLower() : "";
        int count = 0;

        foreach (string product in productList)
        {
            if (selectedProducts.Contains(product)) continue;
            if (!isSearching || product.ToLower().Contains(lowerSearch))
            {
                CreateProductItem(product, productContent.transform);
                if (!isSearching && ++count >= 50) return;
            }
        }
    }

    protected virtual void CreateProductItem(string name, Transform parent, int displayIndex = -1, bool isInMail = false)
    {
        var itemGO = Instantiate(productPrefab, parent);
        var item = itemGO.GetComponent<ProductItem>();
        string displayName = displayIndex > 0 ? $"{displayIndex}. {name}" : name;
        item.Initialize(displayName, name, isInMail);
    }
}
