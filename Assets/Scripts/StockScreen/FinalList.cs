using System;
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


    private Dictionary<string, string[]> finalList = new();

    private string JsonFilePath => Path.Combine(Application.temporaryCachePath, "StockUpdate.json");

    public void OnEnable()
    {
        GameEvents.OnEditToggleClicked += HandleEditClicked;

        exportButton.onClick.AddListener(HandleExportButtonClicked);
    }

    private void OnDisable()
    {
        GameEvents.OnEditToggleClicked -= HandleEditClicked;
        ClearFinalList();

        exportButton.onClick.RemoveAllListeners();
    }

    public override void Show()
    {
        base.Show();
        RefreshList();
    }

    private void RefreshList()
    {
        finalList = JsonUtilityEditor.ReadJson<Dictionary<string, string[]>>(JsonFilePath);

        if (finalList != null)
        {
            string jsonContent = File.ReadAllText(JsonFilePath);
            Debug.Log($"FinalList JSON Content: {jsonContent}");
        }
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
                item.SetData(pair.Key, pair.Value[0], pair.Value[1]);
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

        string excelPath = StockScreen.Instance.ExcelFilePath;
        bool hasExcel = !string.IsNullOrEmpty(excelPath) && !excelPath.Equals("No file selected", StringComparison.OrdinalIgnoreCase);

        string filename = hasExcel ? Path.GetFileName(excelPath) : GetFileName(StockScreen.templateFilePath);

        string userName = PlayerPrefs.GetString("SavedUserName");
        string sheetname = hasExcel ? userName.Split(' ')[0].ToUpper() : null;

        byte[] excelBytes;

        if (hasExcel)
        {
            excelBytes = File.ReadAllBytes(excelPath);
        }
        else
        {
            TextAsset textAsset = Resources.Load<TextAsset>(StockScreen.templateFilePath);
            excelBytes = textAsset.bytes;
        }


        void Finish(string message, bool error = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (error) Debug.LogError(message);
                else Debug.Log(message);
                GUIManager.Instance.ShowAndroidToast(message);
            }
            LoadingScreen.Instance?.Hide();
        }

        ExcelAPI.Instance.ExportExcel(
            excelBytes,
            filename,
            finalList,
            sheetname,
            filePath =>
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Finish("Failed to export final list.", true);
                }
                else
                {
                    DBAPI.Instance.UploadProductData(
                        finalList,
                        () =>
                        {
                            Debug.Log("DB Upload Success");
                            Finish($"Final list exported successfully: {filePath}");
                            Hide();
                            MainMenuPanel.Instance.Show();
                        },
                        err => Debug.LogError("DB Upload Error: " + err)
                    );
                }
            },
            err => Finish(err, true)
        );
    }

    private void HandleEditClicked(FinalListItem item)
    {
        StockUpdatePopup.Instance.Show(item.ProductName);
    }

    public string GetFileName(string filename)
    {
        if (string.IsNullOrEmpty(filename) || filename.Equals("No file selected", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("No Excel file selected.");
            return null;
        }

        string fileName = Path.GetFileNameWithoutExtension(filename);
        string extension = Path.GetExtension(filename);

        // Try to find a month name in the filename
        string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
        string currentMonth = DateTime.Now.ToString("MMMM");

        bool monthFound = false;
        foreach (var month in months)
        {
            if (!string.IsNullOrEmpty(month) && fileName.IndexOf(month, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                fileName = System.Text.RegularExpressions.Regex.Replace(
                    fileName,
                    month,
                    currentMonth,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                monthFound = true;
                break;
            }
        }

        if (!monthFound)
        {
            // If no month found, append current month
            fileName += $"_{currentMonth}";
        }

        Debug.Log($"Updated file name: {fileName + extension}");

        return fileName + extension;
    }
}
