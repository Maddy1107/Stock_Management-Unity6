using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Collapsible : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject prodPrefab;
    [SerializeField] private GameObject prodContainer;
    [SerializeField] private Button sendEmailButton;
    [SerializeField] private Button markallButton;

    private List<DBAPI.ProductRequest> productRequests = new();
    private HashSet<string> receivedProductNames = new();
    private HashSet<int> productIds = new();

    private void OnEnable()
    {
        sendEmailButton.onClick.AddListener(ShowEmailPopup);
        markallButton.onClick.AddListener(MarkAllAsReceived);
        GameEvents.OnMarkedRecieved += OnProductMarkedReceived;
    }

    private void OnDisable()
    {
        sendEmailButton.onClick.RemoveAllListeners();
        markallButton.onClick.RemoveAllListeners();
        GameEvents.OnMarkedRecieved -= OnProductMarkedReceived;
    }

    public void ToggleSection()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void SetProducts(List<DBAPI.ProductRequest> products)
    {
        productRequests = products;
        productIds.Clear();
        receivedProductNames.Clear();

        foreach (var p in products)
        {
            productIds.Add(p.id);
            if (p.received && !string.IsNullOrEmpty(p.product_name))
                receivedProductNames.Add(p.product_name);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in prodContainer.transform)
            Destroy(child.gameObject);

        foreach (var product in productRequests)
        {
            var itemGO = Instantiate(prodPrefab, prodContainer.transform);
            itemGO.transform.SetSiblingIndex(prodContainer.transform.childCount - 1);
            itemGO.GetComponent<ProductRequestItem>()?.Setup(product);
        }
    }

    private void OnProductMarkedReceived(DBAPI.ProductRequest req)
    {
        if (productIds.Contains(req.id) && !string.IsNullOrEmpty(req.product_name))
            receivedProductNames.Add(req.product_name);
    }

    private void ShowEmailPopup()
    {
        if (receivedProductNames.Count == 0)
        {
            GUIManager.Instance.ShowAndroidToast("No received products to include.");
            return;
        }

        FinalProductEmailPopup.Instance.Show();
        FinalProductEmailPopup.Instance.SetEmailContent(
            new List<string>(receivedProductNames),
            "Below are the received products:",
            false,
            "Products received"
        );
    }

    public void MarkAllAsReceived()
    {
        foreach (Transform child in prodContainer.transform)
        {
            if (child.TryGetComponent(out ProductRequestItem item))
            {
                var prod = item.GetProduct();
                if (!prod.received)
                    item.TriggerReceive();
            }
        }

        GUIManager.Instance.ShowAndroidToast("All items marked as received.");
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void OnGUI()
    {
        const int buttonWidth = 160;
        const int buttonHeight = 40;
        Rect rect = new Rect(Screen.width - buttonWidth - 20, Screen.height - buttonHeight - 20, buttonWidth, buttonHeight);

        if (GUI.Button(rect, "Unmark All"))
        {
            UnmarkAllAsReceived();
        }
    }

    private void UnmarkAllAsReceived()
    {
        foreach (Transform child in prodContainer.transform)
        {
            if (child.TryGetComponent(out ProductRequestItem item))
            {
                var prod = item.GetProduct();
                if (prod.received)
                    item.TriggerUnreceive();
            }
        }

        GUIManager.Instance.ShowAndroidToast("All items unmarked.");
    }
#endif
}
