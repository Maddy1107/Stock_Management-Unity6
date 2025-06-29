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
    private const string ExportUrl = "https://backendapi-flask.onrender.com/export";

    public void ExportExcel(TextAsset file, string filename, Dictionary<string, string> data, Action<string> onSuccess, Action<string> onError)
    {
        if (file == null)
        {
            onError?.Invoke("File is null.");
            return;
        }

        string jsonData = JsonConvert.SerializeObject(data);
        StartCoroutine(SendExcel(file, filename, jsonData, onSuccess, onError));
    }

    private IEnumerator SendExcel(TextAsset file, string filename, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", file.bytes, filename + ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.AddField("filename", filename);
        form.AddField("data", jsonData);

        yield return APIClient.PostForm(ExportUrl, form,
            onSuccessBytes =>
            {
                // Save file logic here (Android/Desktop based)
                string savedPath = SaveExportedExcel(filename, onSuccessBytes, out string error);
                if (!string.IsNullOrEmpty(savedPath))
                    onSuccess?.Invoke(savedPath);
                else
                    onError?.Invoke(error);
            },
            onError);
    }

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
