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

    private const string BaseUrl = /*"http://127.0.0.1:5000";*/"https://backendapi-flask.onrender.com";

    private string ExportUrl = $"{BaseUrl}/export";
    private string ExportSheetUrl = $"{BaseUrl}/export-sheet";

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
        string uri;

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", file, filename + ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.AddField("filename", filename);
        form.AddField("data", jsonData);

        if (!string.IsNullOrEmpty(sheetName))
        {
            uri = ExportSheetUrl;
            form.AddField("sheet", sheetName);
        }
        else
        {
            uri = ExportUrl;
        }

        string profileName = PlayerPrefs.GetString("SavedUserName");

        string url = $"{uri}/{profileName}";

        Debug.Log(url);

        yield return APIClient.PostForm(url, form,
                onSuccessBytes =>
                {
                    string savedPath = SaveExportedExcel(filename, onSuccessBytes, out string error);
                    if (!string.IsNullOrEmpty(savedPath))
                        onSuccess?.Invoke(savedPath);
                    else
                        onError?.Invoke(error);
                },
                onError);
    }

    /// <summary>
    /// Saves the returned Excel file to the device.
    /// </summary>
    private string SaveExportedExcel(string filename, byte[] data, out string error)
    {
        error = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        string downloadsPath = $"/storage/emulated/0/Download/ClosingStock/{DateTime.Now:MMMM}";
        string fullPath = Path.Combine(downloadsPath, filename + ".xlsx");
        try
        {
            if (!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            File.WriteAllBytes(fullPath, data);
            return fullPath;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
#elif UNITY_EDITOR
        string savePath = UnityEditor.EditorUtility.SaveFilePanel("Save Excel", "", filename, "xlsx");
        if (!string.IsNullOrEmpty(savePath))
        {
            try
            {
                File.WriteAllBytes(savePath, data);
                return savePath;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }
        error = "User cancelled export.";
        return null;
#else
        error = "Unsupported platform.";
        return null;
#endif
    }
}
