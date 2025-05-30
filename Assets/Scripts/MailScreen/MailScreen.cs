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

    private string emailContent;

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

    public void BuildFullEmail(string content, TMP_Text mailText)
    {
        var sb = new StringBuilder()
            .AppendLine("Dear Team,")
            .AppendLine()
            .AppendLine(content)
            .AppendLine("Thank you \nPriyanka Roy\n");

        emailContent = sb.ToString();
        if (mailText != null)
            mailText.text = emailContent;
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = emailContent ?? string.Empty;
        Debug.Log($"Copied to clipboard:\n{emailContent}");
        GUIManager.Instance.ShowAndroidToast("Copied to clipboard");
    }

    private void ResetMailScreen()
    {
        emailContent = string.Empty;
        productContent?.SetActive(false);
        absentContent?.SetActive(false);

        var productMail = productContent?.GetComponent<ProductMail>();
        productMail?.ResetProductScreen();
    }
}
