using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StockScreen : UIPage<StockScreen>
{
    public static Dictionary<string, string> ProductDictionary { get; private set; } = new Dictionary<string, string>();

    [SerializeField] private FinalList finalList;
    [SerializeField] private StockProductScreen productScreen;

    public static string tempStockUpdatepath { get; private set; }
    public static string templateFilePath { get; private set; }

    public void Show(string stockFilePath)
    {
        Show();
        ResetStockPages();
        Initialize(stockFilePath);
    }

    public void Initialize(string filePath)
    {
        templateFilePath = filePath;
        tempStockUpdatepath = Path.Combine(Application.temporaryCachePath, "StockUpdate.json");
        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(tempStockUpdatepath));
        ProductDictionary.Clear();
    }

    public void UpdateStock(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            Debug.LogError("Key or value cannot be null or empty.");
            return;
        }

        ProductDictionary[key] = $"{value}ml";
        JsonUtilityEditor.WriteJson(tempStockUpdatepath, ProductDictionary);

    }

    public void SubmitStockData()
    {
        if (ProductDictionary == null || ProductDictionary.Count == 0)
        {
            Debug.LogWarning("⚠️ Product dictionary is empty. Nothing to write.");
            GUIManager.Instance.ShowAndroidToast("No products to submit.");
            return;
        }

        JsonUtilityEditor.WriteJson(tempStockUpdatepath, ProductDictionary);
        finalList?.Show();
        productScreen?.Hide();
    }

    public void Reset()
    {
        ProductDictionary.Clear();
        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(tempStockUpdatepath));
    }

    private void ResetStockPages()
    {
        productScreen?.Show();
        finalList?.Hide();
    }

    public void OpenScreen()
    {
        productScreen?.Show();
        finalList?.Hide();
    }
}
