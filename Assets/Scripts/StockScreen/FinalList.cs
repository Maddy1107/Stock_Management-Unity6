using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FinalList : MonoBehaviour
{
    Dictionary<string, string> finalList;

    [SerializeField] private GameObject finalListPrefab;
    [SerializeField] private Transform finalListContainer;
    [SerializeField] private Button updateButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button refreshButton;

    private void Awake()
    {
        finalList = new Dictionary<string, string>();
    }

    void OnEnable()
    {
        ClearChildren(finalListContainer);

        if (updateButton != null)
        {
            updateButton.onClick.AddListener(OnUpdateButtonClicked);
        }

        if (exportButton != null)
        {
            exportButton.onClick.AddListener(OnExportButtonClicked);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(Refresh);
        }

        string filePath = Path.Combine(Application.temporaryCachePath, "StockUpdate.json");
        finalList = JsonUtilityEditor.ReadJson<Dictionary<string, string>>(filePath);

        StockScreen.UpdateSubmitted += Refresh;

        GenerateList();

    }

    private void OnDisable()
    {
        if (updateButton != null)
        {
            updateButton.onClick.RemoveListener(OnUpdateButtonClicked);
        }

        StockScreen.UpdateSubmitted -= Refresh;

        ClearChildren(finalListContainer);
    }

    public void Refresh()
    {
        JsonUtilityEditor.WriteJson(StockScreen.jsonFilepath, StockScreen.productDictionary);

        string filePath = Path.Combine(Application.temporaryCachePath, "StockUpdate.json");
        finalList = JsonUtilityEditor.ReadJson<Dictionary<string, string>>(filePath);

        GenerateList();
    }

    private void OnExportButtonClicked()
    {
        if (finalList == null || finalList.Count == 0)
        {
            Debug.LogWarning("Final list is empty. Nothing to export.");
            GUIManager.Instance.ShowAndroidToast("No data to export.");
            return;
        }

        ExcelReader.Instance.ExportFile(StockScreen.uploadedFilePath, finalList);

        GUIManager.Instance.ShowAndroidToast("Final list exported successfully.");

        //StockScreen.Instance.HandleBackButton();
    }

    private void OnUpdateButtonClicked()
    {
        StockScreen.Instance.OpenScreen();
    }

    private void GenerateList()
    {
        ClearChildren(finalListContainer);

        if (finalList == null || finalList.Count == 0)
        {
            Debug.LogWarning("Final list is empty.");
            return;
        }

        foreach (var item in finalList)
        {
            GameObject listItem = Instantiate(finalListPrefab, finalListContainer);
            FinalListItem finalListItem = listItem.GetComponent<FinalListItem>();
            finalListItem.OnEditToggleClicked += HandleToggleClicked;
            if (finalListItem != null)
            {
                finalListItem.SetData(item.Key, item.Value);
            }
        }
    }
    
    public void HandleToggleClicked(FinalListItem item)
    {
        GUIManager.Instance.ShowStockUpdatePopup(item.ProductName, item.ProductValue);
    }

    private void ClearChildren(Transform finalListContainer)
    {
        if (finalListContainer == null) return;

        for (int i = finalListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(finalListContainer.GetChild(i).gameObject);
        }
    }
}
