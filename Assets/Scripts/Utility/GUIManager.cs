using System;
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

        if (mailScreenGO != null)
            mailScreen = mailScreenGO.GetComponent<MailScreen>();

        if (selectMailPopup != null)
            selectMailPopup.OnTypeSelected = HandleMailTypeSelected;
    }

    private void OnEnable()
    {
        ShowOnly(mainmenuPanelGO);
    }

    private void HandleMailTypeSelected(string type)
    {
        Debug.Log("Mail type selected: " + type);

        if (mailScreen != null)
        {
            mailScreen.OpenWithType(type);
            ShowOnly(mailScreenGO);
        }
    }

    public void ShowSelectMailPopup()
    {
        if (selectMailPopup != null)
            selectMailPopup.gameObject.SetActive(true);
    }

    public void ShowMainMenuPanel(Texture2D texture = null, string name = "")
    {
        if (mainmenuPanelGO != null)
        {
            ShowOnly(mainmenuPanelGO);
            mainmenuPanelGO.GetComponent<MainMenuPanel>().Initialize(texture, name);
        }
    }

    private void ShowOnly(GameObject targetGO)
    {
        if (mainmenuPanelGO != null) mainmenuPanelGO.SetActive(mainmenuPanelGO == targetGO);
        if (mailScreenGO != null) mailScreenGO.SetActive(mailScreenGO == targetGO);
    }

    public void CheckProfileData()
    {
        (string name, Texture2D texture) = startScreenPopup.GetComponent<AboutPanel>().LoadSavedData();
        if (!string.IsNullOrEmpty(name) && texture != null)
        {
            ShowMainMenuPanel(texture, name);
        }
        else
        {
            startScreenPopup.gameObject.SetActive(true);
        }
    }
}
