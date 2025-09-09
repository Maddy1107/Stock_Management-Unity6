using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class ExcelAPI : MonoBehaviour
{
    public static ExcelAPI Instance { get; private set; }

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


    /// <summary>
    /// Exports an Excel file to the server with optional sheet name.
    /// </summary>
    public void ExportExcel(byte[] file, string filename, Dictionary<string, string[]> data, string sheetName = null, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (file == null)
        {
            onError?.Invoke("File is null.");
            return;
        }

        string jsonData = JsonConvert.SerializeObject(data);
        StartCoroutine(SendExcel(file, filename, jsonData, sheetName, onSuccess, onError));
    }

    private IEnumerator SendExcel(byte[] file, string filename, string jsonData, string sheetName, Action<string> onSuccess, Action<string> onError)
    {
        string ExportUrl = $"{APIClient.BaseUrl}/export";
        string ExportSheetUrl = $"{APIClient.BaseUrl}/export-sheet";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", file, filename + ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.AddField("filename", filename);
        form.AddField("data", jsonData);

        string profileName = PlayerPrefs.GetString("SavedUserName");
        string uri = string.IsNullOrEmpty(sheetName) ? ExportUrl : ExportSheetUrl;

        if (!string.IsNullOrEmpty(sheetName))
            form.AddField("sheet", sheetName);

        string url = $"{uri}/{profileName}";
        Debug.Log(url);

        string month = DateTime.Now.ToString("MMMM");

        yield return APIClient.PostForm(url, form,
                onSuccessBytes =>
                {
                    string savedPath = GUIManager.Instance.SaveFile(filename, month, onSuccessBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    if (!string.IsNullOrEmpty(savedPath))
                        onSuccess?.Invoke(savedPath);
                    else
                        onError?.Invoke("Failed");
                },
                onError);
    }
}
