using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Collapsible : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject prodPrefab;
    [SerializeField] private GameObject prodContainer;
    [SerializeField] private Button sendEmailButton;
    [SerializeField] private Button markAllButton;

    private readonly List<DBAPI.ProductRequest> productRequests = new();
    private readonly HashSet<string> receivedProductNames = new();
    private readonly HashSet<int> productIds = new();

    private void OnEnable()
    {
        sendEmailButton.onClick.AddListener(ShowEmailPopup);
        markAllButton.onClick.AddListener(MarkAllAsReceived);
        GameEvents.OnMarkedRecieved += OnProductMarkedReceived;
    }

    private void OnDisable()
    {
        sendEmailButton.onClick.RemoveAllListeners();
        markAllButton.onClick.RemoveAllListeners();
        GameEvents.OnMarkedRecieved -= OnProductMarkedReceived;
    }

    public void ToggleSection()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void SetProducts(List<DBAPI.ProductRequest> products)
    {
        productRequests.Clear();
        productRequests.AddRange(products);

        productIds.Clear();
        receivedProductNames.Clear();

        foreach (var p in productRequests)
        {
            productIds.Add(p.id);
            if (p.received && !string.IsNullOrEmpty(p.product_name))
                receivedProductNames.Add(p.product_name);
        }

        RefreshUI();
        UpdateMarkAllButtonState();
    }

    private void RefreshUI()
    {
        foreach (Transform child in prodContainer.transform)
            Destroy(child.gameObject);

        foreach (var product in productRequests)
        {
            var itemGO = Instantiate(prodPrefab, prodContainer.transform);
            itemGO.GetComponent<ProductRequestItem>()?.Setup(product);
        }
    }

    private void OnProductMarkedReceived(DBAPI.ProductRequest req)
    {
        if (productIds.Contains(req.id) && !string.IsNullOrEmpty(req.product_name))
        {
            receivedProductNames.Add(req.product_name);
            UpdateMarkAllButtonState();
        }
    }

    private void UpdateMarkAllButtonState()
    {
        bool allMarked = true;

        foreach (var product in productRequests)
        {
            if (!product.received)
            {
                allMarked = false;
                break;
            }
        }

        markAllButton.interactable = !allMarked;
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
        BatchToggle(
            matchCondition: item => !item.GetProduct().received,
            action: item => item.MarkReceivedSilently,
            "All items already marked received.",
            finalCallback: () =>
            {
                Debug.Log("✅ All items marked as received.");
                UpdateMarkAllButtonState();
                GUIManager.Instance.ShowAndroidToast("All items marked received!");
            }
        );
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void OnGUI()
    {
        const int width = 160, height = 40;
        Rect rect = new(Screen.width - width - 20, Screen.height - height - 20, width, height);

        if (GUI.Button(rect, "Unmark All"))
            MarkAllAsNotReceived();
    }

    public void MarkAllAsNotReceived()
    {
        BatchToggle(
            matchCondition: item => item.GetProduct().received,
            action: item => item.UnmarkReceivedSilently,
            "No items are marked as received.",
            finalCallback: () =>
        {
            Debug.Log("✅ All items marked as not received.");
            UpdateMarkAllButtonState();
            GUIManager.Instance.ShowAndroidToast("All items marked not received!");
        }
        );
    }
#endif

    private void BatchToggle(
    System.Predicate<ProductRequestItem> matchCondition,
    System.Func<ProductRequestItem, System.Action<System.Action>> action,
    string noMatchToast,
    System.Action finalCallback = null)
    {
        int total = 0, completed = 0;

        foreach (Transform child in prodContainer.transform)
        {
            if (child.TryGetComponent(out ProductRequestItem item) && matchCondition(item))
            {
                total++;
                action(item)(() =>
                {
                    completed++;
                    if (completed >= total)
                    {
                        LoadingScreen.Instance.Hide();
                        finalCallback?.Invoke();
                    }
                });
            }
        }

        if (total > 0)
        {
            LoadingScreen.Instance.Show();
        }
        else
        {
            GUIManager.Instance.ShowAndroidToast(noMatchToast);
            finalCallback?.Invoke(); // Optional: also run callback if nothing to do
        }
    }

}
