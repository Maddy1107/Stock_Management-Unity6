using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonUtilityReader
{
    /// <summary>
    /// Reads a product list from a JSON file in the Resources folder.
    /// </summary>
    /// <param name="fileName">Name of the JSON file (without extension).</param>
    /// <returns>List of product names, or null if loading fails.</returns>
    public static List<string> ReadProductJson(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            Debug.LogError("Invalid file name for product JSON.");
            return null;
        }

        TextAsset jsonAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(fileName));
        if (jsonAsset == null)
        {
            Debug.LogError($"JSON file not found in Resources: {fileName}");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<List<string>>(jsonAsset.text);
        }
        catch (JsonException e)
        {
            Debug.LogError($"Failed to parse JSON content: {e.Message}");
            return null;
        }
    }
}
