using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonUtilityReader
{
    public static List<string> ReadProductJson(string fileName)
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("JSON file not found at: " + jsonPath);
            return null;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        return JsonConvert.DeserializeObject<List<string>>(jsonContent);
    }
}
