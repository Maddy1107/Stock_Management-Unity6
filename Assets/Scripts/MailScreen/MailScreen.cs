using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum MailType
{
    Required,
    Received,
    Absent
}

public class MailScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button copyButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button updateButton;
    [SerializeField] private GameObject productContent;
    [SerializeField] private GameObject absentContent;
    private string emailContent;

    private void OnEnable()
    {
        ResetMailScreen();
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (copyButton != null)
        {
            copyButton.onClick.RemoveAllListeners();
            copyButton.onClick.AddListener(CopyToClipboard);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HandleBackButton);
        }

        if (updateButton != null)
        {
            updateButton.onClick.RemoveAllListeners();
            updateButton.onClick.AddListener(OpenProductContent);
        }
    }

    private void HandleBackButton()
    {
        gameObject.SetActive(false);
        productContent?.SetActive(false);
        absentContent?.SetActive(false);
        ResetMailScreen();
        GUIManager.Instance.ShowMainMenuPanel();
    }

    private void OpenProductContent()
    {
        if (productContent != null)
        {
            var productMail = productContent.GetComponentInParent<ProductMail>();
            if (productMail != null)
                productMail.OpenScreen();
        }
    }

    public void Open(MailType type)
    {
        ResetMailScreen();

        switch (type)
        {
            case MailType.Required:
            case MailType.Received:
                if (productContent != null)
                {
                    var productMail = productContent.GetComponentInParent<ProductMail>();
                    if (productMail != null)
                        productMail.OpenWithType(type);
                }
                break;
            case MailType.Absent:
                if (absentContent != null)
                    absentContent.SetActive(true);
                break;
        }
    }

    public void BuildFullEmail(string content,TMP_Text mailtext)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Dear Team,");
        sb.AppendLine();
        sb.AppendLine(content);
        sb.AppendLine("Thank you \nPriyanka Roy");
        sb.AppendLine();

        emailContent = sb.ToString();
        if (mailtext != null)
            mailtext.text = emailContent;
    }

    private void CopyToClipboard()
    {
    #if UNITY_EDITOR
        GUIUtility.systemCopyBuffer = emailContent ?? string.Empty;
        Debug.Log($"Copied to clipboard:\n{emailContent}");

    #elif UNITY_ANDROID
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject clipboardService = activity.Call<AndroidJavaObject>("getSystemService", "clipboard"))
            using (AndroidJavaObject clipData = new AndroidJavaClass("android.content.ClipData")
                    .CallStatic<AndroidJavaObject>("newPlainText", "label", emailContent ?? string.Empty)))
            {
                clipboardService.Call("setPrimaryClip", clipData);
                Debug.Log($"Copied to clipboard:\n{emailContent}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to copy to clipboard on Android: " + e.Message);
        }

    #elif UNITY_IOS
        // You will need a native iOS plugin to support clipboard on iOS.
        Debug.LogWarning("Copy to clipboard on iOS requires native plugin support.");

    #else
        Debug.LogWarning("Copy to clipboard is not supported on this platform.");
    #endif
    }


    public void ResetMailScreen()
    {
        emailContent = string.Empty;
        productContent?.SetActive(false);
        absentContent?.SetActive(false);
        if (productContent != null)
        {
            var productMail = productContent.GetComponent<ProductMail>();
            if (productMail != null)
                productMail.ResetProductScreen();
        }
    }
}
