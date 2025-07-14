using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockScreen : UIPage<StockScreen>
{
    public static Dictionary<string, string[]> ProductDictionary { get; private set; } = new Dictionary<string, string[]>();

    public static string tempStockUpdatepath { get; private set; }
    public static string templateFilePath { get; private set; }
    public string ExcelFilePath
    {
        get { return excelFilePath != null ? excelFilePath.text : string.Empty; }
        private set { if (excelFilePath != null) excelFilePath.text = value; }
    }

    private List<string> addedProducts;

    [SerializeField] private TMP_Text excelFilePath;
    [SerializeField] private Button uploadButton;

    void OnEnable()
    {
        Reset();
        uploadButton.onClick.AddListener(HandleUploadButtonClicked);
    }

    void OnDisable()
    {
        uploadButton.onClick.RemoveAllListeners();
    }

    public void Show(string stockFilePath)
    {
        Show();
        Initialize(stockFilePath);
    }

    public void Initialize(string filePath)
    {
        templateFilePath = filePath;
        tempStockUpdatepath = Path.Combine(Application.temporaryCachePath, "StockUpdate.json");
        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(tempStockUpdatepath));
        ProductDictionary.Clear();
    }

    public void UpdateStock(string key, string value, string bottles)
    {
        if (string.IsNullOrWhiteSpace(key) || value == null || value.Length == 0)
        {
            Debug.LogError("Key or value cannot be null or empty.");
            return;
        }

        int bottleCount;
        if (int.TryParse(bottles, out bottleCount))
        {
            string bottleLabel = bottleCount == 1 ? "bottle" : "bottles";
            ProductDictionary[key] = new string[]
            {
                $"{value[0]}ml",
                $"{bottles} {bottleLabel}"
            };
        }
        else
        {
            Debug.LogError("Invalid bottle count.");
        }

        JsonUtilityEditor.WriteJson(tempStockUpdatepath, ProductDictionary);
    }

    public void SubmitStockData()
    {
        if (ProductDictionary == null || ProductDictionary.Count == 0)
        {
            Debug.LogWarning("Product dictionary is empty. Nothing to write.");
            GUIManager.Instance.ShowAndroidToast("No products to submit.");
            return;
        }

        JsonUtilityEditor.WriteJson(tempStockUpdatepath, ProductDictionary);
        FinalList.Instance?.Show();
    }

    public void Reset()
    {
        excelFilePath.text = "No file selected";
        ProductDictionary.Clear();
        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(tempStockUpdatepath));
    }

    private void HandleUploadButtonClicked()
    {
        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                excelFilePath.text = "No file selected";
                Debug.Log("File selection cancelled");
                return;
            }

            Debug.Log("Excel file picked at: " + path);
            HandlePickedExcelFile(path);
        },
        new string[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-excel" }); // .xlsx, .xls
    }

    private void HandlePickedExcelFile(string filePath)
    {
        if (excelFilePath != null)
        {
            excelFilePath.text = Path.GetFileName(filePath);
        }
    }
}
