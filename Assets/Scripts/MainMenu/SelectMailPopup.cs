using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectMailPopup : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeButton;
    public Button requireButton;
    public Button receiveButton;

    public Action<string> OnTypeSelected;

    void OnEnable()
    {
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);
        if (requireButton != null) requireButton.onClick.AddListener(OnRequireButtonClicked);
        if (receiveButton != null) receiveButton.onClick.AddListener(OnReceiveButtonClicked);
    }

    void OnDisable()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        if (requireButton != null) requireButton.onClick.RemoveListener(OnRequireButtonClicked);
        if (receiveButton != null) receiveButton.onClick.RemoveListener(OnReceiveButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnRequireButtonClicked()
    {
        OnTypeSelected?.Invoke("required");
        ClosePopup();
    }

    private void OnReceiveButtonClicked()
    {
        OnTypeSelected?.Invoke("received");
        ClosePopup();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
