using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectMailPopup : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button requireButton;
    [SerializeField] private Button receiveButton;
    [SerializeField] private Button absentButton;

    public Action<MailType> OnTypeSelected;

    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);
        if (requireButton != null) requireButton.onClick.AddListener(() => OnMailTypeSelected(MailType.Required));
        if (receiveButton != null) receiveButton.onClick.AddListener(() => OnMailTypeSelected(MailType.Received));
        if (absentButton != null) absentButton.onClick.AddListener(() => OnMailTypeSelected(MailType.Absent));
    }

    private void RemoveListeners()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(ClosePopup);
        if (requireButton != null) requireButton.onClick.RemoveAllListeners();
        if (receiveButton != null) receiveButton.onClick.RemoveAllListeners();
        if (absentButton != null) absentButton.onClick.RemoveAllListeners();
    }

    private void OnMailTypeSelected(MailType type)
    {
        OnTypeSelected?.Invoke(type);
        ClosePopup();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
