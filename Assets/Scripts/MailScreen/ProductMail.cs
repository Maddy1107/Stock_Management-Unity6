using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ProductMail : ProductListbase
{
    [Header("UI References")]
    [SerializeField] private GameObject mailContent;
    [SerializeField] private Button buildEmailButton;
    [SerializeField] private Button copyButton;
    [SerializeField] private TMP_Text mailText;

    private Transform mailParent;
    private MailType currentMailType;
    private string headerText;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (mailParent == null)
            mailParent = mailContent.transform;

        buildEmailButton.onClick.AddListener(BuildEmailBody);
        GameEvents.OnToggleClicked += HandleToggleClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        buildEmailButton.onClick.RemoveListener(BuildEmailBody);
        GameEvents.OnToggleClicked -= HandleToggleClicked;
    }

    public void Show(MailType type)
    {
        gameObject.SetActive(true);
        OpenWithType(type);
    }
    public void Hide() => gameObject.SetActive(false);

    public void OpenWithType(MailType type)
    {
        currentMailType = type;
        headerText = $"Below are the {(type == MailType.Required ? "required" : "received")} products:";
        ResetProductScreen();
    }

    public void BuildEmailBody()
    {
        if (selectedProducts.Count == 0)
        {
            GUIManager.Instance.ShowAndroidToast("No products selected for the email.");
            return;
        }

        FinalProductEmailPopup.Instance.Show();
        FinalProductEmailPopup.Instance.SetEmailContent(selectedProducts,headerText, $"Product {currentMailType}");
    }

    private void RefreshMailList()
    {
        ClearChildren(mailParent);

        int index = 1;
        foreach (var product in selectedProducts)
        {
            CreateProductItem(product, mailParent, index++, true);
        }
    }


    protected override void CreateProductItem(string name, Transform parent, int displayIndex = -1, bool isInMail = false)
    {
        base.CreateProductItem(name, parent, displayIndex, isInMail);
    }

    private void HandleToggleClicked(ProductItem item)
    {
        if (item == null) return;

        string name = item.ProductName;

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
        Transform content = productContent.transform;

        for (int i = 0; i < content.childCount; i++)
        {
            var item = content.GetChild(i).GetComponent<ProductItem>();
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
    }
}
