using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductRequestScreen : UIPage<ProductRequestScreen>
{
    [Header("UI References")]
    [SerializeField] private GameObject productRequestPrefab;
    [SerializeField] private Transform productContainer;
    [SerializeField] private TMP_Text noDataText;

    void OnEnable()
    {
        noDataText.gameObject.SetActive(true);
        ClearContainer();
    }

    public override void Show()
    {
        base.Show();
        GetProductRequests();
    }

    private void GetProductRequests()
    {
        LoadingScreen.Instance.Show();

        DBAPI.Instance.FetchRequestedProducts(
            onSuccess: (requestGroups) =>
            {
                PopulateRequests(requestGroups);
                LoadingScreen.Instance.Hide();
            },
            onError: (error) =>
            {
                Debug.LogError("Failed to fetch requested products: " + error);
                GUIManager.Instance.ShowAndroidToast("Failed to fetch requested products.");
                LoadingScreen.Instance.Hide();
            }
        );
    }

    private void PopulateRequests(List<DBAPI.RequestGroup> groups)
    {
        ClearContainer();

        bool hasData = groups != null && groups.Count > 0;
        noDataText.gameObject.SetActive(!hasData);

        if (!hasData) return;

        foreach (var group in groups)
        {
            var groupGO = Instantiate(productRequestPrefab, productContainer);
            var groupUI = groupGO.GetComponent<ProductDateGroupItem>();

            if (groupUI != null)
                groupUI.Setup(group);
        }
    }

    private void ClearContainer()
    {
        foreach (Transform child in productContainer)
            Destroy(child.gameObject);
    }
}
