using TMPro;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private SelectMailPopup selectMailPopup;
    [SerializeField] private GameObject startScreenPopup;
    [SerializeField] private GameObject mailScreenGO;
    [SerializeField] private GameObject mainMenuPanelGO;

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

    public void ShowFinalEmailScreen(string emailContent, TMP_Text mailText = null)
    {
        mailScreen?.BuildFullEmail(emailContent, mailText);
    }

    public void ShowSelectMailPopup()
    {
        if (selectMailPopup != null)
            selectMailPopup.GetComponent<PopupAnimator>().Show();
    }

    public void ShowMainMenuPanel(Texture2D texture = null, string name = "")
    {
        if (!mainMenuPanelGO) return;

        ShowOnly(mainMenuPanelGO);
        var mainMenuPanel = mainMenuPanelGO.GetComponent<MainMenuPanel>();
        mainMenuPanel?.Initialize(texture, name);
    }

    private void ShowOnly(GameObject targetGO)
    {
        if (mainMenuPanelGO != null)
            mainMenuPanelGO.SetActive(mainMenuPanelGO == targetGO);

        if (mailScreenGO != null)
            mailScreenGO.SetActive(mailScreenGO == targetGO);
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

        (string name, Texture2D texture) = aboutPanel.LoadSavedData();
        if (!string.IsNullOrWhiteSpace(name) && texture != null)
        {
            ShowMainMenuPanel(texture, name);
        }
        else
        {
            startScreenPopup.GetComponent<PopupAnimator>().Show();
        }
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
