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

    public event Action<MailType> OnTypeSelected;

    private void OnEnable()
    {
        closeButton?.onClick.AddListener(ClosePopup);
        requireButton?.onClick.AddListener(() => SelectType(MailType.Required));
        receiveButton?.onClick.AddListener(() => SelectType(MailType.Received));
        absentButton?.onClick.AddListener(() => SelectType(MailType.Absent));
    }

    private void OnDisable()
    {
        closeButton?.onClick.RemoveListener(ClosePopup);
        requireButton?.onClick.RemoveAllListeners();
        receiveButton?.onClick.RemoveAllListeners();
        absentButton?.onClick.RemoveAllListeners();
    }

    private void SelectType(MailType type)
    {
        ClosePopup();
        OnTypeSelected?.Invoke(type);
    }

    private void ClosePopup()
    {
        GetComponent<PopupAnimator>()?.Hide();
    }
}
