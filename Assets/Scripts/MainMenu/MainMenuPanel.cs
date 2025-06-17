using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : UIPage<MainMenuPanel>
{
    [Header("Buttons")]
    [SerializeField] private Button mailButton, stockButton, imagesButton;

    [Header("Profile UI")]
    [SerializeField] private TMP_Text profileName;

    private bool hasCheckedProfileData = false;

    private const string StockFilePath = "Assets/Resources/Priyanka_Closing_Stock_May_2025.xlsx";

    public void Initialize()
    {
        CheckProfileData();
    }

    public void CheckProfileData()
    {
        if (hasCheckedProfileData) return;

        hasCheckedProfileData = true;

        string name = AboutPanel.Instance?.LoadSavedData();

        if (!string.IsNullOrWhiteSpace(name))
        {
            profileName.text = name;
        }
        else
        {
            AboutPanel.Instance?.Show();
        }
        
    }

    private void OnEnable()
    {
        Initialize();
        mailButton?.onClick.AddListener(() => SelectMailPopup.Instance?.Show());
        stockButton?.onClick.AddListener(() => StockScreen.Instance?.Show(StockFilePath));
        imagesButton?.onClick.AddListener(() => ImageUploadPopup.Instance?.Show());
    }

    private void OnDisable()
    {
        mailButton?.onClick.RemoveAllListeners();
        stockButton?.onClick.RemoveAllListeners();
        imagesButton?.onClick.RemoveAllListeners();
    }
}
