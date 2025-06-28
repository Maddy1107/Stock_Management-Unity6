using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FinalList : UIPopup<FinalList>
{
    [Header("UI References")]
    [SerializeField] private GameObject finalListPrefab;
    [SerializeField] private Transform finalListContainer;
    [SerializeField] private Button exportButton;

    private Dictionary<string, string> finalList = new();

    private string JsonFilePath => Path.Combine(Application.temporaryCachePath, "StockUpdate.json");

    public void OnEnable()
    {
        exportButton.onClick.AddListener(HandleExportButtonClicked);
        GameEvents.OnEditToggleClicked += HandleEditClicked;
        GameEvents.OnUpdateSubmitted += RefreshList;
    }

    private void OnDisable()
    {
        exportButton.onClick.RemoveListener(HandleExportButtonClicked);
        GameEvents.OnEditToggleClicked -= HandleEditClicked;
        GameEvents.OnUpdateSubmitted -= RefreshList;
        ClearFinalList();
    }

    public override void Show()
    {
        base.Show();
        RefreshList();
    }

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

    private void HandleExportButtonClicked()
    {
        LoadingScreen.Instance?.Show();

        if (finalList == null || finalList.Count == 0)
        {
            GUIManager.Instance.ShowAndroidToast("No data to export.");
            LoadingScreen.Instance?.Hide();
            return;
        }

        var excelAsset = Resources.Load<TextAsset>(StockScreen.templateFilePath);

        void HideLoadingAndToast(string message, bool isError = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (isError)
                    Debug.LogError(message);
                else
                    Debug.Log(message);

                GUIManager.Instance.ShowAndroidToast(message);
            }
            LoadingScreen.Instance?.Hide();
        }

        ExcelReader.Instance.ExportFile(
            excelAsset,
            StockScreen.templateFilePath,
            finalList,
            filePath =>
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    HideLoadingAndToast("Failed to export final list.", true);
                }
                else
                {
                    HideLoadingAndToast($"Final list exported successfully: {filePath}");
                    Hide();
                    MainMenuPanel.Instance.Show();
                }
            },
            error =>
            {
                HideLoadingAndToast(error, true);
            }
        );
    }

    private void HandleEditClicked(FinalListItem item)
    {
        StockUpdatePopup.Instance.Show(item.ProductName, item.ProductValue);
    }
}
