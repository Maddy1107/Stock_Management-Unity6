using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonUtilityReader
{
    public static Dictionary<string, List<string>> ReadCategoryJson(string fileName)
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("JSON file not found at: " + jsonPath);
            return null;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonContent);
    }
}
