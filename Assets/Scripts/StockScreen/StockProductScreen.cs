using UnityEngine;
using UnityEngine.UI;

public class StockProductScreen : ProductListbase
{
    [SerializeField] private Button submitButton;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            
        ResetProductScreen();
    }

    protected override void OnDisable()
    {
        if (submitButton != null)
            submitButton.onClick.RemoveAllListeners();
    }

    
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void OnSubmitButtonClicked()
    {
        StockScreen.Instance.SubmitStockData();
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
        StockUpdatePopup.Instance.Show(item.ProductName);
    }

    public void ResetProductScreen()
    {
        StockScreen.Instance.Reset();
    }
}
