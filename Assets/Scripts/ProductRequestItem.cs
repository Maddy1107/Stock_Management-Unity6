using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ProductRequestItem : MonoBehaviour
{
    [SerializeField] private TMP_Text prod_name;
    [SerializeField] private Toggle receivedToggle;
    [SerializeField] private TMP_Text status;

    private DBAPI.ProductRequest currentProduct;

    void OnEnable() => receivedToggle.onValueChanged.AddListener(OnToggleChanged);
    void OnDisable() => receivedToggle.onValueChanged.RemoveListener(OnToggleChanged);

    public void Setup(DBAPI.ProductRequest product)
    {
        currentProduct = product;
        prod_name.text = product.product_name;
        receivedToggle.isOn = product.received;
        receivedToggle.interactable = !product.received;
        status.text = product.received
            ? $"Received on\n{FormatDate(product.received_at)}"
            : "Not received";
    }

    private void OnToggleChanged(bool isOn)
    {
        if (currentProduct == null || currentProduct.received || !isOn)
        {
            receivedToggle.isOn = currentProduct?.received ?? false;
            return;
        }

        LoadingScreen.Instance.Show();

        DBAPI.Instance.MarkRequestReceived(currentProduct.id,
            () =>
            {
                currentProduct.received = true;
                currentProduct.received_at = FormatDate(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                status.text = $"Received on\n{FormatDate(currentProduct.received_at)}";
                receivedToggle.interactable = false;

                GameEvents.InvokeOnMarkRecieved(currentProduct);
                LoadingScreen.Instance.Hide();
            },
            error =>
            {
                Debug.LogError("Failed to mark as received: " + error);
                GUIManager.Instance.ShowAndroidToast("Network Error!! Please try later.");
                receivedToggle.isOn = false;
                LoadingScreen.Instance.Hide();
            });
    }

    private string FormatDate(string isoDate) =>
        DateTime.TryParse(isoDate, out var parsed) ? parsed.ToString("MMM dd, yyyy") : isoDate;

    public DBAPI.ProductRequest GetProduct() => currentProduct;

    public void TriggerReceive()
    {
        if (!receivedToggle.isOn && receivedToggle.interactable)
            receivedToggle.isOn = true; // This will trigger OnToggleChanged
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public void TriggerUnreceive() => OnToggleChanged(false);
#endif
}
