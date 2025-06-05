using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StockScreen : ProductListbase
{
    public static StockScreen Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public static Dictionary<string, string> productDictionary = new Dictionary<string, string>();

    [SerializeField] private Button submitButton;
    [SerializeField] private Button backButton;
    ExcelResponse rows;
    public static string jsonFilepath;
    public static string uploadedFilePath;
    [SerializeField] private GameObject finalList;
    [SerializeField] private GameObject productScreen;

    protected override void OnEnable()
    {
        base.OnEnable();

        ResetProductScreen();

        jsonFilepath = Path.Combine(Application.temporaryCachePath, "StockUpdate.json");

        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(jsonFilepath));

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
            GUIManager.Instance.ShowAndroidToast("No products to submit.");
            return;
        }

        JsonUtilityEditor.WriteJson(jsonFilepath, productDictionary);

        finalList.SetActive(true);
        productScreen.SetActive(false);

    }

    public void OpenScreen()
    {
        productScreen.SetActive(true);
        finalList.SetActive(false);
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
            productDictionary[key] = $"{value}ml";
        }
        else
        {
            productDictionary.Add(key, $"{value}ml");
        }
    }

    public void Initialize(string filePath)
    {
        uploadedFilePath = filePath;
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
        string result = rows.productData.TryGetValue(item.ProductName, out string quantity) ? quantity : "0";
        GUIManager.Instance.ShowStockUpdatePopup(item.ProductName, result);
    }

    public void HandleBackButton()
    {
        JsonUtilityEditor.DeleteFileFromTempCache(Path.GetFileNameWithoutExtension(jsonFilepath));
        gameObject.SetActive(false);
        GUIManager.Instance.ShowMainMenuPanel();
    }

    public void ReadExcel(string filePath)
    {
        ExcelReader.Instance.UploadFile(filePath, 
            onCompleted: (response) =>
            {
                rows = response;
            },
            onError: (errorMsg) =>
            {
                Debug.LogError("Upload error: " + errorMsg);
            }
        );
    }

    public void ResetProductScreen()
    {
        productDictionary.Clear();
        finalList.SetActive(false);
        productScreen.SetActive(true);
    }
}
