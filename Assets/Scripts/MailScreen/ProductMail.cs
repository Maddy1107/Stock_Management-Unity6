using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProductMail : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mailContent;
    [SerializeField] private GameObject productContent;
    [SerializeField] private GameObject productPrefab;
    [SerializeField] private GameObject finalMailContent;
    [SerializeField] private GameObject productSelectContent;
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button clearSearchButton;
    [SerializeField] private Button buildEmailButton;
    [SerializeField] private Button copyButton;
    [SerializeField] private TMP_Text mailText;

    private List<string> productList;
    private readonly HashSet<string> selectedProducts = new();
    private Transform productParent;
    private Transform mailParent;
    private string headerText;

    private void OnEnable()
    {
        OpenScreen();

        productList = JsonUtilityReader.ReadProductJson("product_list");
        productParent = productContent.transform;
        mailParent = mailContent.transform;

        SetupUIListeners();
        GenerateAllProductLists();
    }

    private void OnDisable()
    {
        RemoveUIListeners();
    }

    private void SetupUIListeners()
    {

        searchInputField.onValueChanged.AddListener(SearchProducts);
        clearSearchButton.onClick.AddListener(ClearSearch);
        buildEmailButton.onClick.AddListener(BuildEmailBody);
        copyButton.onClick.AddListener(() => MailScreen.Instance.CopyToClipboard());
    }

    private void RemoveUIListeners()
    {
        searchInputField.onValueChanged.RemoveAllListeners();
        clearSearchButton.onClick.RemoveAllListeners();
        buildEmailButton.onClick.RemoveAllListeners();
        copyButton.onClick.RemoveAllListeners();
    }

    public void OpenScreen()
    {
        productSelectContent.SetActive(true);
        finalMailContent.SetActive(false);
    }

    public void OpenWithType(MailType type)
    {
        productSelectContent.SetActive(true);
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

        bool isSearching = !string.IsNullOrWhiteSpace(searchText);
        string lowerSearch = isSearching ? searchText.ToLower() : "";
        int count = 0;

        foreach (string product in productList)
        {
            if (selectedProducts.Contains(product)) continue;
            if (!isSearching || product.ToLower().Contains(lowerSearch))
            {
                CreateProductItem(product, productParent, false);
                if (!isSearching && ++count >= 50) return;
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
        var itemGO = Instantiate(productPrefab, parent);
        var item = itemGO.GetComponent<ProductItem>();
        string displayName = displayIndex > 0 ? $"{displayIndex}. {name}" : name;

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
            var item = productParent.GetChild(i).GetComponent<ProductItem>();
            if (item != null && item.ProductName == name)
            {
                Destroy(item.gameObject);
                break;
            }
        }
    }

    private void ClearChildren(Transform parent)
    {
        if (!parent) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void ClearSearch()
    {
        searchInputField.text = "";
        PopulateProductList(null);
    }

    public void BuildEmailBody()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(headerText))
        {
            sb.AppendLine(headerText).AppendLine();
        }

        int index = 1;
        foreach (var name in selectedProducts)
        {
            sb.AppendLine($"{index++}. {name}");
        }

        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString(), mailText);
        productSelectContent.SetActive(false);
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
