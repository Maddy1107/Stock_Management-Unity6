using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class DBAPI : MonoBehaviour
{
    public static DBAPI Instance { get; private set; }

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
    private const string BaseUrl = "https://backendapi-flask.onrender.com";

    public void UploadProductData(Dictionary<string, string[]> data, Action onSuccess, Action<string> onError)
    {
        string month = DateTime.Now.ToString("MMMM");
        string year = DateTime.Now.ToString("yyyy");

        string url = $"{BaseUrl}/month-data?month={month}&year={year}";
        string json = JsonConvert.SerializeObject(data);

        StartCoroutine(APIClient.PostJSON(url, json,
            response =>
            {
                Debug.Log("DB Upload Success: " + response);
                GUIManager.Instance.ShowAndroidToast("Data uploaded successfully.");
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError("DB Upload Error: " + error);
                GUIManager.Instance.ShowAndroidToast("Data upload error.");
                onError?.Invoke(error);
            }));
    }

    public void FetchProductData(string month, string year, Action<Dictionary<string, List<string>>> onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/month-data/{month}?year={year}";

        StartCoroutine(APIClient.GetJSON(url,
            response =>
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject<MonthResponse>(response);
                    var result = new Dictionary<string, List<string>>();
                    foreach (var p in parsed.products)
                    {
                        result[p.product] = new List<string> { p.quantity, p.bottles };
                    }
                    onSuccess?.Invoke(result);

                }
                catch (Exception ex)
                {
                    onError?.Invoke("Parsing failed: " + ex.Message);
                }
            },
            error =>
            {
                Debug.LogError("DB Fetch Error: " + error);
                onError?.Invoke(error);
            }));
    }

    [Serializable]
    public class ProductEntry
    {
        public string product;
        public string quantity;
        public string bottles;
    }

    [Serializable]
    public class MonthResponse
    {
        public List<ProductEntry> products;
    }
}
