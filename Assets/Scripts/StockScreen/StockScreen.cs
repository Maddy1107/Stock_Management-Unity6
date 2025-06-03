using System;
using System.Collections.Generic;
using System.IO;
using MiniExcelLibs;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;

public class StockScreen : ProductListbase
{
    private static Dictionary<string, string> productDictionary = new Dictionary<string, string>();

    [SerializeField] private Button submitButton;
    [SerializeField] private Button backButton;
    List<IDictionary<string, object>> rows = new List<IDictionary<string, object>>();
    string filePath;

    protected override void OnEnable()
    {
        base.OnEnable();

        filePath = Path.Combine(Application.temporaryCachePath, "StockUpdate.json");

        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(filePath));

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButton);
        }
    }

    private void OnSubmitButtonClicked()
    {
        if (productDictionary == null || productDictionary.Count == 0)
        {
            Debug.LogWarning("⚠️ Product dictionary is empty. Nothing to write.");
            return;
        }

        JsonUtilityEditor.WriteJson(filePath, productDictionary);

        // Debug log to confirm
        if (File.Exists(filePath))
        {
            Debug.Log($"✅ JSON file exists after write: {filePath}");

            string jsonContent = File.ReadAllText(filePath);
            Debug.Log($"📄 JSON content:\n{jsonContent}");
        }
        else
        {
            Debug.LogError($"❌ Failed to create or locate the file at: {filePath}");
        }
    }


    public static void StockUpdate(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            Debug.LogError("Key or value cannot be null or empty.");
            return;
        }

        if (productDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"Key '{key}' already exists in the dictionary. Updating value.");
            productDictionary[key] = value;
        }
        else
        {
            productDictionary.Add(key, value);
        }
    }

    public void Initialize(string filePath)
    {
        ReadExcel(filePath);
    }

    protected override void CreateProductItem(string name, Transform parent, int displayIndex = -1, bool isInMail = false)
    {
        var itemGO = Instantiate(productPrefab, parent);
        var item = itemGO.GetComponent<ProductItem>();
        string displayName = displayIndex > 0 ? $"{displayIndex}. {name}" : name;
        item.Initialize(displayName, name, isInMail);
        item.OnToggleClicked += HandleToggleClicked;
    }

    private void HandleToggleClicked(ProductItem item)
    {
        string result = ExcelReader.FindProductValue(rows, item.ProductName);
        GUIManager.Instance.ShowStockUpdatePopup(item.ProductName,result);
    }
    
    private void HandleBackButton()
    {
        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(filePath));
        gameObject.SetActive(false);
        GUIManager.Instance.ShowMainMenuPanel();
    }

    public void ReadExcel(string filePath)
    {
        rows = ExcelReader.ReadExcelRows(filePath);
    }

}
