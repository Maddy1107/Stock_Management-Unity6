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

    public string uploadApiUrl = "http://127.0.0.1:5000/upload";
    public string exportApiUrl = "http://127.0.0.1:5000/export";

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

    // public void UploadFile(string filePath, Action<ExcelResponse> onCompleted, Action<string> onError = null)
    // {
    //     if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
    //     {
    //         onError?.Invoke("Invalid file path: " + filePath);
    //         return;
    //     }

    //     StartCoroutine(UploadFileCoroutine(filePath, onCompleted, onError));
    // }

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
        if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
        {
            onError?.Invoke("File not found: " + filepath);
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filepath);
        string fileNameOnly = Path.GetFileName(saveFileName); // What you want the downloaded file to be called

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filepath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.AddField("filename", fileNameOnly);
        form.AddField("data", jsondata);

        using (UnityWebRequest www = UnityWebRequest.Post(exportApiUrl, form))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string currMonth = GetCurrentMonth();

#if UNITY_ANDROID && !UNITY_EDITOR
                    // Android: save to Downloads
                    string downloadsPath = $"/storage/emulated/0/Download/ClosingStock/{currMonth}"; // common location
                    string destFile = Path.Combine(downloadsPath, saveFileName);
                    if (!Directory.Exists(downloadsPath))
                    {
                        Directory.CreateDirectory(downloadsPath);
                    }
                    try
                    {
                        File.WriteAllBytes(destFile, www.downloadHandler.data);
                        Debug.Log($"Export successful! File saved to: {destFile}");
                        onSuccess?.Invoke(destFile);
                    }
                    catch (Exception e)
                    {
                        string errorMsg = $"Failed to save file to {destFile}: {e.Message}";
                        Debug.LogError(errorMsg);
                        onError?.Invoke(errorMsg);
                    }
#elif UNITY_EDITOR || UNITY_STANDALONE
#if UNITY_EDITOR
                // Unity Editor: show Save File Panel
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
                        Debug.Log($"Export successful! File saved to: {savePath}");
                        onSuccess?.Invoke(savePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("❌ Failed to export on Editor: " + e.Message);

                        onError?.Invoke("Failed to export: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("Export cancelled by user.");
                    onError?.Invoke("Export cancelled by user.");
                }
#endif
#else
                        Debug.LogWarning("❌ Export not supported on this platform.");
#endif
            }
            else
            {
                string errorMsg = $"Export failed: {www.error}\n{www.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }
    }  


    // private IEnumerator UploadFileCoroutine(string path, Action<ExcelResponse> onCompleted, Action<string> onError)
    // {
    //     WWWForm form = CreateUploadForm(path);

    //     using (UnityWebRequest www = UnityWebRequest.Post(uploadApiUrl, form))
    //     {
    //         www.SetRequestHeader("Accept", "application/json");

    //         Debug.Log("Uploading file: " + Path.GetFileName(path));
    //         yield return www.SendWebRequest();

    //         if (www.result != UnityWebRequest.Result.Success)
    //         {
    //             string error = "Upload failed: " + www.error + "\nResponse: " + www.downloadHandler.text;
    //             Debug.LogError(error);
    //             onError?.Invoke(error);
    //         }
    //         else
    //         {
    //             string jsonResponse = www.downloadHandler.text;
    //             Debug.Log("Upload successful! Server response:\n" + jsonResponse);

    //             try
    //             {
    //                 var wrapper = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonResponse);
    //                 ExcelResponse response = new ExcelResponse();
    //                 response.productData = wrapper != null && wrapper.ContainsKey("data") ? wrapper["data"] : new Dictionary<string, string>();
    //                 onCompleted?.Invoke(response);
    //             }
    //             catch (Exception e)
    //             {
    //                 string parseError = "Failed to parse JSON response: " + e.Message;
    //                 Debug.LogError(parseError);
    //                 onError?.Invoke(parseError);
    //             }
    //         }
    //     }
    // }

    // private static WWWForm CreateUploadForm(string filePath)
    // {
    //     byte[] fileData = File.ReadAllBytes(filePath);
    //     string fileName = Path.GetFileName(filePath);

    //     WWWForm form = new WWWForm();
    //     form.AddBinaryData("file", fileData, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    //     return form;
    // }

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
