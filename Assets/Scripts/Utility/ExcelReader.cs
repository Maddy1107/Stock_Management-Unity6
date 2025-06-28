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

#if UNITY_EDITOR
    private string exportApiUrl = "https://backendapi-flask.onrender.com/export";
#else
    private string exportApiUrl = "https://backendapi-flask.onrender.com/export";
#endif

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

    public void ExportFile(TextAsset file, string filename, Dictionary<string, string> jsondata, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (file == null)
        {
            onError?.Invoke("File is null.");
            return;
        }
        string jsonString = JsonConvert.SerializeObject(jsondata);
        StartCoroutine(Export(file, GetFileNameWithNewMonth(filename), jsonString, onSuccess, onError));
    }

    private IEnumerator Export(TextAsset file, string saveFileName, string jsondata, Action<string> onSuccess, Action<string> onError)
    {
        byte[] fileData = file.bytes;
        string fileNameOnly = Path.GetFileName(saveFileName);
        string filenameWithExtension = saveFileName + ".xlsx";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, saveFileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.AddField("filename", fileNameOnly);
        form.AddField("data", jsondata);

        using (UnityWebRequest www = UnityWebRequest.Post(exportApiUrl, form))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                string downloadsPath = $"/storage/emulated/0/Download/ClosingStock/{GetCurrentMonth()}";
                string destFile = Path.Combine(downloadsPath, filenameWithExtension);
                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);
                if (File.Exists(destFile))
                    File.Delete(destFile);
                try
                {
                    File.WriteAllBytes(destFile, www.downloadHandler.data);
                    onSuccess?.Invoke(destFile);
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Failed to save file: {e.Message}");
                }
#elif UNITY_EDITOR
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
                        onSuccess?.Invoke(savePath);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke("Failed to export: " + e.Message);
                    }
                }
                else
                {
                    onError?.Invoke("Export cancelled by user.");
                }
#endif
            }
            else
            {
                onError?.Invoke($"Export failed: {www.error}\n{www.downloadHandler.text}");
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
