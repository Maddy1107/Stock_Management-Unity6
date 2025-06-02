using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MailType
{
    Required,
    Received,
    Absent
}

public class MailScreen : MonoBehaviour
{
    public static MailScreen Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("UI References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button updateButton;
    [SerializeField] private GameObject productContent;
    [SerializeField] private GameObject absentContent;

    private string subjectText;
    private string emailBodyText;

    private void OnEnable()
    {
        ResetMailScreen();
        SetupButtonListeners();
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveListener(HandleBackButton);
        updateButton.onClick.RemoveListener(OpenProductContent);
    }

    private void SetupButtonListeners()
    {
        backButton.onClick.AddListener(HandleBackButton);
        updateButton.onClick.AddListener(OpenProductContent);
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
        var productMail = productContent?.GetComponentInParent<ProductMail>();
        productMail?.OpenScreen();
    }

    public void Open(MailType type)
    {
        ResetMailScreen();

        switch (type)
        {
            case MailType.Required:
            case MailType.Received:
                var productMail = productContent?.GetComponentInParent<ProductMail>();
                productMail?.OpenWithType(type);
                break;

            case MailType.Absent:
                absentContent?.SetActive(true);
                break;
        }
    }

    public void BuildFullEmail(string content, TMP_Text mailText, string subject)
    {
        subjectText = subject;

        var sb = new StringBuilder()
            .AppendLine("Dear Team,")
            .AppendLine()
            .AppendLine(content)
            .AppendLine("Thank you \nPriyanka Roy\n");

        emailBodyText = sb.ToString();
        if (mailText != null)
            mailText.text = emailBodyText;

        Debug.Log($"Email body built:\n{emailBodyText}");
        Debug.Log($"Email subject: {subjectText}");
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = subjectText + "\n\n" + emailBodyText ?? string.Empty;
        Debug.Log($"Copied to clipboard:\n{emailBodyText}");
        GUIManager.Instance.ShowAndroidToast("Copied to clipboard");

        //OpenOutlook(subjectText, emailBodyText);
    }

    public static void OpenOutlook(string subject, string body)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            Debug.Log("[EmailUtility] Attempting to open Outlook...");

            string outlookPackage = "com.microsoft.office.outlook";

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
            using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.SEND"))
            {
                Debug.Log("[EmailUtility] Intent created with ACTION_SEND");

                intent.Call<AndroidJavaObject>("setType", "message/rfc822");
                Debug.Log("[EmailUtility] MIME type set to message/rfc822");

                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.SUBJECT", subject);
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", body);
                Debug.Log($"[EmailUtility] Subject: {subject}");
                Debug.Log($"[EmailUtility] Body: {body}");

                intent.Call<AndroidJavaObject>("setPackage", outlookPackage);
                Debug.Log("[EmailUtility] Outlook package set: com.microsoft.office.outlook");

                // Verify Outlook is available
                AndroidJavaObject resolveInfo = packageManager.Call<AndroidJavaObject>("resolveActivity", intent, 0);
                if (resolveInfo == null)
                {
                    Debug.LogWarning("[EmailUtility] Outlook not installed or cannot handle intent.");
                    return;
                }

                Debug.Log("[EmailUtility] Outlook is available. Launching intent...");
                activity.Call("startActivity", intent);
                Debug.Log("[EmailUtility] Outlook intent launched successfully.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[EmailUtility] Exception occurred: " + ex.Message);
        }
#else
        Debug.Log("[EmailUtility] This function only works on a real Android device.");
#endif
    }


    private void ResetMailScreen()
    {
        emailBodyText = string.Empty;
        productContent?.SetActive(false);
        absentContent?.SetActive(false);

        var productMail = productContent?.GetComponent<ProductMail>();
        productMail?.ResetProductScreen();
    }
}
