using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonUtilityReader
{
    public static List<string> ReadProductJson(string fileName)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(fileName));
        if (jsonAsset == null)
        {
            Debug.LogError("JSON file not found in Resources: " + fileName);
            return null;
        }
        string jsonContent = jsonAsset.text;
        return JsonConvert.DeserializeObject<List<string>>(jsonContent);
    }
}
