using UnityEngine;
using UnityEngine.UI;

public class SelectMailPopup : UIPopup<SelectMailPopup>
{
    [SerializeField] private Button requireButton;
    [SerializeField] private Button absentButton;

    private void OnEnable()
    {
        requireButton?.onClick.AddListener(() => HandleClick(MailType.Required));
        absentButton?.onClick.AddListener(() => HandleClick(MailType.Absent));
    }

    private void OnDisable()
    {
        requireButton?.onClick.RemoveAllListeners();
        absentButton?.onClick.RemoveAllListeners();
    }

    private void HandleClick(MailType type)
    {
        MailScreen.Instance?.Show();
        GameEvents.InvokeOnTypeSelected(type);
        Hide();
    }
}
