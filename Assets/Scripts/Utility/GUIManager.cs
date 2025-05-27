using System;
using TMPro;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private SelectMailPopup selectMailPopup;
    [SerializeField] private GameObject startScreenPopup;
    [SerializeField] private GameObject mailScreenGO;
    [SerializeField] private GameObject mainmenuPanelGO;

    private MailScreen mailScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mailScreen = mailScreenGO ? mailScreenGO.GetComponent<MailScreen>() : null;

        if (selectMailPopup)
            selectMailPopup.OnTypeSelected = HandleMailTypeSelected;
    }

    private void OnEnable()
    {
        ShowOnly(mainmenuPanelGO);
    }

    private void HandleMailTypeSelected(MailType type)
    {
        Debug.Log($"Mail type selected: {type}");

        if (mailScreen)
        {
            ShowOnly(mailScreenGO);
            mailScreen.Open(type);
        }
    }

    public void ShowFinalEmailScreen(string emailContent,TMP_Text mailText = null)
    {
        mailScreen?.BuildFullEmail(emailContent, mailText);
    }

    public void ShowSelectMailPopup()
    {
        if (selectMailPopup)
            selectMailPopup.gameObject.SetActive(true);
    }

    public void ShowMainMenuPanel(Texture2D texture = null, string name = "")
    {
        if (!mainmenuPanelGO) return;

        ShowOnly(mainmenuPanelGO);
        var mainMenuPanel = mainmenuPanelGO.GetComponent<MainMenuPanel>();
        if (mainMenuPanel)
            mainMenuPanel.Initialize(texture, name);
    }

    private void ShowOnly(GameObject targetGO)
    {
        if (mainmenuPanelGO)
            mainmenuPanelGO.SetActive(mainmenuPanelGO == targetGO);
        if (mailScreenGO)
            mailScreenGO.SetActive(mailScreenGO == targetGO);
    }

    public void CheckProfileData()
    {
        var aboutPanel = startScreenPopup ? startScreenPopup.GetComponent<AboutPanel>() : null;
        if (aboutPanel == null)
        {
            Debug.LogWarning("AboutPanel not found on startScreenPopup.");
            return;
        }

        (string name, Texture2D texture) = aboutPanel.LoadSavedData();
        if (!string.IsNullOrEmpty(name) && texture)
        {
            ShowMainMenuPanel(texture, name);
        }
        else
        {
            if (startScreenPopup)
                startScreenPopup.SetActive(true);
        }
    }
}
