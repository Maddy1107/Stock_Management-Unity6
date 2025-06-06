using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductMail : ProductListbase
{
    [SerializeField] private GameObject mailContent;
    [SerializeField] private GameObject finalMailContent;
    [SerializeField] private GameObject productSelectContent;
    [SerializeField] private Button buildEmailButton;
    [SerializeField] private Button copyButton;
    [SerializeField] private TMP_Text mailText;

    private Transform mailParent;
    private MailType currentMailType;
    private string headerText;

    protected override void OnEnable()
    {
        base.OnEnable();
        mailParent = mailContent.transform;
        buildEmailButton.onClick.AddListener(BuildEmailBody);
        copyButton.onClick.AddListener(() => MailScreen.Instance.CopyToClipboard());
        RefreshMailList();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        buildEmailButton.onClick.RemoveAllListeners();
        copyButton.onClick.RemoveAllListeners();
        finalMailContent.SetActive(false);
    }

    public void OpenScreen()
    {
        productSelectContent.SetActive(true);
        finalMailContent.SetActive(false);
    }

    public void OpenWithType(MailType type)
    {
        currentMailType = type;
        productSelectContent.SetActive(true);
        headerText = $"Below are the {(type == MailType.Required ? "required" : "received")} products:";
        ResetProductScreen();
    }

    public void BuildEmailBody()
    {
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrWhiteSpace(headerText))
        {
            sb.AppendLine(headerText).AppendLine();
        }

        if (selectedProducts.Count == 0)
        {
            GUIManager.Instance.ShowAndroidToast("No products selected for the email.");
            return;
        }

        int index = 1;
        foreach (var name in selectedProducts)
        {
            sb.AppendLine($"{index++}. {name}");
        }

        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString(), mailText, $"Product {currentMailType}");
        productSelectContent.SetActive(false);
        finalMailContent.SetActive(true);
    }

    private void RefreshMailList()
    {
        ClearChildren(mailParent);
        int index = 1;
        foreach (var name in selectedProducts)
        {
            CreateProductItem(name, mailParent, index++, true);
        }
    }

    protected override void CreateProductItem(string name, Transform parent, int displayIndex = -1, bool isInMail = false)
    {
        var itemGO = Instantiate(productPrefab, parent);
        var item = itemGO.GetComponent<ProductItem>();
        string displayName = displayIndex > 0 ? $"{displayIndex}. {name}" : name;
        item.Initialize(displayName, name, isInMail);
        GameEvents.OnToggleClicked += HandleToggleClicked;
    }

    private void HandleToggleClicked(ProductItem item)
    {
        string name = item.ProductName;
        GameEvents.OnToggleClicked -= HandleToggleClicked;

        if (item.IsInMail)
        {
            selectedProducts.Remove(name);
            Destroy(item.gameObject);
            RefreshMailList();
            CreateProductItem(name, productContent.transform, -1, false);
        }
        else
        {
            selectedProducts.Add(name);
            Destroy(item.gameObject);
            CreateProductItem(name, mailParent, mailParent.childCount + 1, true);
            RemoveProductFromList(name);
        }
    }

    private void RemoveProductFromList(string name)
    {
        for (int i = 0; i < productContent.transform.childCount; i++)
        {
            var item = productContent.transform.GetChild(i).GetComponent<ProductItem>();
            if (item != null && item.ProductName == name)
            {
                Destroy(item.gameObject);
                break;
            }
        }
    }

    public void ResetProductScreen()
    {
        selectedProducts.Clear();
        ClearSearch();
        RefreshMailList();
        finalMailContent.SetActive(false);
    }
}
