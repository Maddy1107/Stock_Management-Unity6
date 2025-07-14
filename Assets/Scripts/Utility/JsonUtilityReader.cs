using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonUtilityEditor
{
    /// <summary>
    /// Reads a JSON file from any full path and deserializes it to the given type.
    /// </summary>
    public static T ReadJson<T>(string fullFilePath)
    {
        if (string.IsNullOrWhiteSpace(fullFilePath))
        {
            Debug.LogError("File path is null or empty.");
            return default;
        }

        if (!File.Exists(fullFilePath))
        {
            Debug.LogError($"File does not exist at: {fullFilePath}");
            return default;
        }

        try
        {
            string json = File.ReadAllText(fullFilePath);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading JSON from file: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Writes an object as JSON to any full file path.
    /// </summary>
    public static void WriteJson<T>(string fullFilePath, T data)
    {
        if (string.IsNullOrWhiteSpace(fullFilePath))
        {
            Debug.LogError(" File path is null or empty.");
            return;
        }

        if (data == null)
        {
            Debug.LogError(" Data is null, cannot write to file.");
            return;
        }

        try
        {
            string directory = Path.GetDirectoryName(fullFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(fullFilePath, json);
            Debug.Log($"JSON written to: {fullFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($" Error writing JSON to file: {ex.Message}");
        }
    }

    public static T ReadJsonFromResources<T>(string fileNameWithoutExtension)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(fileNameWithoutExtension);

        if (jsonAsset == null)
        {
            Debug.LogError($" JSON file not found in Resources: {fileNameWithoutExtension}");
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(jsonAsset.text);
        }
        catch (JsonException e)
        {
            Debug.LogError($" Failed to parse JSON: {e.Message}");
            return default;
        }
    }

    public static void DeleteFileFromTempCache(string fileName)
    {
        string fullPath = Path.Combine(Application.temporaryCachePath, fileName);

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                Debug.Log($"Deleted file: {fullPath}");
            }
            catch (IOException e)
            {
                Debug.LogError($" Failed to delete file: {fullPath}. Exception: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"File not found to delete: {fullPath}");
        }
    }
}
