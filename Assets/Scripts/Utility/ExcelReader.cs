using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

[Serializable]
public class ExcelResponse
{
    public Dictionary<string, string> productData;
}

public class ExcelReader : MonoBehaviour
{
    public static ExcelReader Instance { get; private set; }

    private string exportApiUrl = "http://127.0.0.1:5000/export";

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

    public void ExportFile(string filePath, Dictionary<string, string> jsondata, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            onError?.Invoke("Invalid file path: " + filePath);
            return;
        }

        // Convert jsondata dictionary to JSON string before passing to Export
        string jsonString = JsonConvert.SerializeObject(jsondata);
        StartCoroutine(Export(filePath, GetFileNameWithNewMonth(filePath), jsonString, onSuccess, onError));
    }

    private IEnumerator Export(string filepath, string saveFileName, string jsondata, Action<string> onSuccess, Action<string> onError)
    {
        Debug.Log($"[Export] Starting export for file: {filepath}, save as: {saveFileName}");

        if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
        {
            Debug.LogError($"[Export] File not found: {filepath}");
            onError?.Invoke("File not found: " + filepath);
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filepath);
        Debug.Log($"[Export] Read {fileData.Length} bytes from {filepath}");

        string fileNameOnly = Path.GetFileName(saveFileName);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filepath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.AddField("filename", fileNameOnly);
        form.AddField("data", jsondata);

        Debug.Log($"[Export] Sending POST request to {exportApiUrl} with file: {fileNameOnly}");

        using (UnityWebRequest www = UnityWebRequest.Post(exportApiUrl, form))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            Debug.Log($"[Export] POST request completed. Result: {www.result}");

            if (www.result == UnityWebRequest.Result.Success)
            {
                string currMonth = GetCurrentMonth();
                Debug.Log($"[Export] Server responded successfully. Preparing to save file for month: {currMonth}");

#if UNITY_ANDROID && !UNITY_EDITOR
                string downloadsPath = $"/storage/emulated/0/Download/ClosingStock/{currMonth}";
                string destFile = Path.Combine(downloadsPath, saveFileName);
                Debug.Log($"[Export] Android save path: {destFile}");
                if (!Directory.Exists(downloadsPath))
                {
                    Directory.CreateDirectory(downloadsPath);
                    Debug.Log($"[Export] Created directory: {downloadsPath}");
                }
                try
                {
                    File.WriteAllBytes(destFile, www.downloadHandler.data);
                    Debug.Log($"[Export] Export successful! File saved to: {destFile}");
                    onSuccess?.Invoke(destFile);
                }
                catch (Exception e)
                {
                    string errorMsg = $"[Export] Failed to save file to {destFile}: {e.Message}";
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
#elif UNITY_EDITOR || UNITY_STANDALONE
#if UNITY_EDITOR
                Debug.Log("[Export] Running in Unity Editor. Showing Save File Panel.");
                string savePath = UnityEditor.EditorUtility.SaveFilePanel(
                    "Save Exported Excel File",
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    saveFileName,
                    "xlsx"
                );

                if (!string.IsNullOrEmpty(savePath))
                {
                    try
                    {
                        File.WriteAllBytes(savePath, www.downloadHandler.data);
                        Debug.Log($"[Export] Export successful! File saved to: {savePath}");
                        onSuccess?.Invoke(savePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Export] ❌ Failed to export on Editor: " + e.Message);
                        onError?.Invoke("Failed to export: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("[Export] Export cancelled by user.");
                    onError?.Invoke("Export cancelled by user.");
                }
#endif
#else
                Debug.LogWarning("[Export] ❌ Export not supported on this platform.");
#endif
            }
            else
            {
                string errorMsg = $"[Export] Export failed: {www.error}\n{www.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }
    }

    private string GetCurrentMonth()
    {
        return DateTime.Now.ToString("MMMM");
    }

    public int FindMonthIndex(string fileName, out string foundMonth)
    {
        string[] months = new[]
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        for (int i = 0; i < months.Length; i++)
        {
            if (fileName.IndexOf(months[i], StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                foundMonth = months[i];
                return i;
            }
        }
        foundMonth = null;
        return -1;
    }

    public string GetFileNameWithNewMonth(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        string foundMonth;
        int foundMonthIndex = FindMonthIndex(fileName, out foundMonth);

        if (foundMonthIndex == -1)
        {
            // No month found, return original file name
            return fileName + extension;
        }

        string nextMonth = GetCurrentMonth();

        string newFileName = System.Text.RegularExpressions.Regex.Replace(
            fileName,
            foundMonth,
            nextMonth,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        return newFileName + extension;
    }
    
}
