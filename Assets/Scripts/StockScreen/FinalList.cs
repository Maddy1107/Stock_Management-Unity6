using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FinalList : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject finalListPrefab;
    [SerializeField] private Transform finalListContainer;
    [SerializeField] private Button updateButton;
    [SerializeField] private Button exportButton;

    private Dictionary<string, string> finalList = new();

    private string JsonFilePath => Path.Combine(Application.temporaryCachePath, "StockUpdate.json");

    private void Awake()
    {
        updateButton.onClick.AddListener(HandleUpdateButtonClicked);
        exportButton.onClick.AddListener(HandleExportButtonClicked);
        GameEvents.OnEditToggleClicked += HandleEditClicked;
        GameEvents.OnUpdateSubmitted += RefreshList;
    }

    private void OnDestroy()
    {
        updateButton.onClick.RemoveListener(HandleUpdateButtonClicked);
        exportButton.onClick.RemoveListener(HandleExportButtonClicked);
        GameEvents.OnEditToggleClicked -= HandleEditClicked;
        GameEvents.OnUpdateSubmitted -= RefreshList;
    }

    private void OnEnable()
    {
        RefreshList();
    }

    private void OnDisable()
    {
        ClearFinalList();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void RefreshList()
    {
        finalList = JsonUtilityEditor.ReadJson<Dictionary<string, string>>(JsonFilePath);
        GenerateFinalListUI();
    }

    private void GenerateFinalListUI()
    {
        ClearFinalList();

        if (finalList == null || finalList.Count == 0)
        {
            Debug.Log("Final list is empty.");
            return;
        }

        foreach (var pair in finalList)
        {
            var listItemGO = Instantiate(finalListPrefab, finalListContainer);
            if (listItemGO.TryGetComponent(out FinalListItem item))
            {
                item.SetData(pair.Key, pair.Value);
            }
        }
    }

    private void ClearFinalList()
    {
        for (int i = finalListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(finalListContainer.GetChild(i).gameObject);
        }
    }

    private void HandleUpdateButtonClicked()
    {
        StockScreen.Instance.OpenScreen(); // Decouple if needed
    }

    private void HandleExportButtonClicked()
    {
        if (finalList == null || finalList.Count == 0)
        {
            GUIManager.Instance.ShowAndroidToast("No data to export.");
            return;
        }

        ExcelReader.Instance.ExportFile(StockScreen.templateFilePath, finalList,
            (filePath) =>
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    GUIManager.Instance.ShowAndroidToast("Failed to export final list.");
                    return;
                }

                Debug.Log($"Final list exported successfully: {filePath}");
                
                GUIManager.Instance.OpenFolder(filePath);

                GUIManager.Instance.ShowAndroidToast($"Final list exported successfully: {filePath}");
            });
    }

    private void HandleEditClicked(FinalListItem item)
    {
        StockUpdatePopup.Instance.Show(item.ProductName, item.ProductValue);
    }
}
