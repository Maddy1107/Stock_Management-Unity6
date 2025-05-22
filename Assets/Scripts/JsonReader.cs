using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JsonReader : MonoBehaviour
{
    [System.Serializable]
    public class CategoryData : Dictionary<string, List<string>> {}

    private string fileName = "closing_stock_categories.json";

    void Start()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            Dictionary<string, List<string>> data = JsonUtilityWrapper.FromJson<CategoryData>(jsonContent);

            foreach (var category in data)
            {
                Debug.Log($"Category: {category.Key}, Product Count: {category.Value.Count}");
            }
        }
        else
        {
            Debug.LogError("JSON file not found at: " + jsonPath);
        }
    }
}

public static class JsonUtilityWrapper
{
    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}

