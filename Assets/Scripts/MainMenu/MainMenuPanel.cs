using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : UIPage<MainMenuPanel>
{
    [Header("Buttons")]
    [SerializeField] private Button mailButton, stockButton, imagesButton, editNameButton, previousStockButton, scheduleButton;

    [Header("Profile UI")]
    [SerializeField] private TMP_Text profileName;

    private const string StockFilePath = "Priyanka_Closing_Stock_May_2025";

    protected override void Awake()
    {
        base.Awake();
        InitializeProfileName();
    }
    private void InitializeProfileName()
    {
        if (profileName == null)
        {
            Debug.LogWarning("Profile name text is not assigned.");
            return;
        }

        string savedName = AboutPanel.Instance?.LoadSavedData();

        if (!string.IsNullOrWhiteSpace(savedName))
        {
            profileName.text = savedName;
        }
        else
        {
            profileName.text = "Guest";
        }
    }

    public void SetProfileName(string name)
    {
        if (profileName == null)
        {
            Debug.LogWarning("Profile name text is not assigned.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            profileName.text = name;
        }
        else
        {
            profileName.text = "Guest";
        }
    }

    private void OnEnable()
    {
        mailButton?.onClick.AddListener(() => SelectMailPopup.Instance?.Show());
        stockButton?.onClick.AddListener(() => StockScreen.Instance?.Show(StockFilePath));
        imagesButton?.onClick.AddListener(() => ImageUploadPopup.Instance?.Show());
        editNameButton?.onClick.AddListener(() => AboutPanel.Instance?.Show());
        previousStockButton?.onClick.AddListener(() => PreviousStock.Instance?.Show());
        scheduleButton?.onClick.AddListener(() => ProductRequestScreen.Instance?.Show());
    }

    private void OnDisable()
    {
        mailButton?.onClick.RemoveAllListeners();
        stockButton?.onClick.RemoveAllListeners();
        imagesButton?.onClick.RemoveAllListeners();
        editNameButton?.onClick.RemoveAllListeners();
        previousStockButton?.onClick.RemoveAllListeners();
        scheduleButton?.onClick.RemoveAllListeners();
    }
}
