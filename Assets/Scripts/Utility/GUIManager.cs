using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private SelectMailPopup selectMailPopup;
    [SerializeField] private GameObject startScreenPopup;
    [SerializeField] private GameObject excelUploadPopup;
    [SerializeField] private GameObject stockUpdatePopup;
    [SerializeField] private GameObject imageUploadPopup;

    [SerializeField] private GameObject mailScreenGO;
    [SerializeField] private GameObject mainMenuPanelGO;
    [SerializeField] private GameObject stockScreenGO;

    private MailScreen mailScreen;
    private bool hasCheckedProfileData = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mailScreen = mailScreenGO?.GetComponent<MailScreen>();

        if (selectMailPopup != null)
            selectMailPopup.OnTypeSelected += HandleMailTypeSelected;
    }

    private void OnEnable()
    {
        ShowOnly(mainMenuPanelGO);
    }

    private void HandleMailTypeSelected(MailType type)
    {
        Debug.Log($"Mail type selected: {type}");
        if (mailScreen != null)
        {
            ShowOnly(mailScreenGO);
            mailScreen.Open(type);
        }
    }

    public void ShowFinalEmailScreen(string emailContent, TMP_Text mailText = null, string subject = "")
    {
        mailScreen?.BuildFullEmail(emailContent, mailText, subject);
    }

    public void ShowSelectMailPopup()
    {
        if (selectMailPopup != null)
            selectMailPopup.GetComponent<PopupAnimator>().Show();
    }

    public void ShowStockUpdatePopup(string productName = "", string productQuan = "")
    {
        if (stockUpdatePopup != null)
        {
            stockUpdatePopup.GetComponent<PopupAnimator>().Show();
            var stockUpdate = stockUpdatePopup.GetComponent<StockUpdatePopup>();
            if (stockUpdate != null)
            {
                stockUpdate.Initialize(productName, productQuan);
            }
        }
    }

    public void ShowMainMenuPanel(string name = "")
    {
        if (!mainMenuPanelGO) return;

        ShowOnly(mainMenuPanelGO);
        var mainMenuPanel = mainMenuPanelGO.GetComponent<MainMenuPanel>();
        mainMenuPanel?.Initialize(name);
    }

    public void ShowStockScreen(string filePath = "")
    {
        if (!stockScreenGO) return;

        ShowOnly(stockScreenGO);

        var stockScreen = stockScreenGO.GetComponent<StockScreen>();
        if (stockScreen != null)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                stockScreen.Initialize(filePath);
            }
        }
    }

    private void ShowOnly(GameObject targetGO)
    {
        foreach (var go in new[] { mainMenuPanelGO, mailScreenGO, stockScreenGO })
        {
            if (go != null)
                go.SetActive(go == targetGO);
        }
    }

    public void CheckProfileData()
    {
        if (hasCheckedProfileData) return;
        hasCheckedProfileData = true;

        if (startScreenPopup == null) return;

        var aboutPanel = startScreenPopup.GetComponent<AboutPanel>();
        if (aboutPanel == null)
        {
            Debug.LogWarning("AboutPanel not found on startScreenPopup.");
            return;
        }

        string name = aboutPanel.LoadSavedData();
        if (!string.IsNullOrWhiteSpace(name))
        {
            ShowMainMenuPanel(name);
        }
        else
        {
            startScreenPopup.GetComponent<PopupAnimator>().Show();
        }
    }

    public void ShowExcelUploadPopup()
    {
        excelUploadPopup?.GetComponent<PopupAnimator>()?.Show();
    }

    public void ShowImageUploadPopup()
    {
        imageUploadPopup?.GetComponent<PopupAnimator>()?.Show();
    }

    public void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass toastClass = new("android.widget.Toast");
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                toast.Call("show");
            }));
        }
#else
        Debug.Log("Toast: " + message);
#endif
    }
}
