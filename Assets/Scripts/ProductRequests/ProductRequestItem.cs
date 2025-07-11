using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ProductRequestItem : MonoBehaviour
{
    [SerializeField] private TMP_Text prodNameText;
    [SerializeField] private TMP_Text statusText;
    public Toggle receivedToggle;

    private DBAPI.ProductRequest currentProduct;

    private void OnEnable() => receivedToggle.onValueChanged.AddListener(OnToggleChanged);
    private void OnDisable() => receivedToggle.onValueChanged.RemoveListener(OnToggleChanged);

    public void Setup(DBAPI.ProductRequest product)
    {
        currentProduct = product;
        prodNameText.text = product.product_name;

        receivedToggle.isOn = product.received;
        receivedToggle.interactable = !product.received;

        statusText.text = product.received
            ? $"Received on\n{FormatDate(product.received_at)}"
            : "Not received";
    }

    private void OnToggleChanged(bool isOn)
    {
        if (currentProduct == null) return;

        if (isOn && !currentProduct.received)
        {
            MarkReceived();
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        else if (!isOn && currentProduct.received)
        {
            UnmarkReceived();
        }
#else
        else if (!isOn && currentProduct.received)
        {
            receivedToggle.isOn = true;
            GUIManager.Instance.ShowAndroidToast("Cannot unmark in release build.");
        }
#endif
    }

    private void MarkReceived()
    {
        LoadingScreen.Instance.Show();

        DBAPI.Instance.MarkRequestReceived(currentProduct.id, () =>
        {
            currentProduct.received = true;
            currentProduct.received_at = FormatDate(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

            receivedToggle.isOn = true;
            receivedToggle.interactable = false;
            statusText.text = $"Received on\n{FormatDate(currentProduct.received_at)}";

            GameEvents.InvokeOnMarkRecieved(currentProduct);
            LoadingScreen.Instance.Hide();
        },
        error =>
        {
            Debug.LogError("Failed to mark as received: " + error);
            receivedToggle.isOn = false;
            GUIManager.Instance.ShowAndroidToast("Network Error!! Please try later.");
            LoadingScreen.Instance.Hide();
        });
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void UnmarkReceived()
    {
        LoadingScreen.Instance.Show();

        DBAPI.Instance.MarkRequestNotReceived(currentProduct.id, () =>
        {
            currentProduct.received = false;
            currentProduct.received_at = null;

            receivedToggle.isOn = false;
            receivedToggle.interactable = true;
            statusText.text = "Not received";

            GameEvents.InvokeOnMarkRecieved(currentProduct);
            LoadingScreen.Instance.Hide();
        },
        error =>
        {
            Debug.LogError("Failed to unmark: " + error);
            receivedToggle.isOn = true;
            GUIManager.Instance.ShowAndroidToast("Failed to unmark");
            LoadingScreen.Instance.Hide();
        });
    }

    public void TriggerUnreceive() => OnToggleChanged(false);

    public void UnmarkReceivedSilently(Action onComplete)
    {
        if (currentProduct == null || !currentProduct.received)
        {
            onComplete?.Invoke();
            return;
        }

        DBAPI.Instance.MarkRequestNotReceived(currentProduct.id, () =>
        {
            currentProduct.received = false;
            currentProduct.received_at = null;

            receivedToggle.isOn = false;
            receivedToggle.interactable = true;
            statusText.text = "Not received";

            GameEvents.InvokeOnMarkRecieved(currentProduct);
            onComplete?.Invoke();
        },
        error =>
        {
            Debug.LogError("Failed to unmark: " + error);
            receivedToggle.isOn = true;
            GUIManager.Instance.ShowAndroidToast("Some items failed to unmark.");
            onComplete?.Invoke();
        });
    }
#endif

    public void MarkReceivedSilently(Action onComplete)
    {
        if (currentProduct == null || currentProduct.received)
        {
            onComplete?.Invoke();
            return;
        }

        DBAPI.Instance.MarkRequestReceived(currentProduct.id, () =>
        {
            currentProduct.received = true;
            currentProduct.received_at = FormatDate(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

            receivedToggle.isOn = true;
            receivedToggle.interactable = false;
            statusText.text = $"Received on\n{FormatDate(currentProduct.received_at)}";

            GameEvents.InvokeOnMarkRecieved(currentProduct);
            onComplete?.Invoke();
        },
        error =>
        {
            Debug.LogError("Failed to mark as received: " + error);
            receivedToggle.isOn = false;
            GUIManager.Instance.ShowAndroidToast("Some items failed to mark.");
            onComplete?.Invoke();
        });
    }

    public void TriggerReceive() => OnToggleChanged(true);
    public DBAPI.ProductRequest GetProduct() => currentProduct;
    private string FormatDate(string isoDate) =>
        DateTime.TryParse(isoDate, out var parsed) ? parsed.ToString("MMM dd, yyyy") : isoDate;
}
