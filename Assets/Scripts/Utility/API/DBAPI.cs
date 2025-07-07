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

    private const string BaseUrl = /*"http://127.0.0.1:5000";*/"https://backendapi-flask.onrender.com";

    // 1. Upload monthly product data
    public void UploadProductData(Dictionary<string, string[]> data, Action onSuccess, Action<string> onError)
    {
        string month = DateTime.Now.ToString("MMM").ToUpper();
        string year = DateTime.Now.ToString("yyyy");

        string url = $"{BaseUrl}/month-data?month={month}&year={year}";
        string json = JsonConvert.SerializeObject(data);

        StartCoroutine(APIClient.PostJSON(url, json,
            response => { Debug.Log("Upload success"); onSuccess?.Invoke(); },
            error => { Debug.LogError("Upload error: " + error); onError?.Invoke(error); }
        ));
    }

    // 2. Fetch monthly product data
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
            error => { Debug.LogError("Fetch error: " + error); onError?.Invoke(error); }
        ));
    }

    // 3. Request products (new table)
    public void RequestProducts(List<string> productNames, Action onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/request-products";

        var body = new List<SimpleProductRequest>();
        foreach (var name in productNames)
        {
            body.Add(new SimpleProductRequest { product_name = name });
        }

        string json = JsonConvert.SerializeObject(body);

        StartCoroutine(APIClient.PostJSON(url, json,
            response => { Debug.Log("Request sent"); onSuccess?.Invoke(); },
            error => { Debug.LogError("Request error: " + error); onError?.Invoke(error); }
        ));
    }

    // 4. Fetch all requested products (grouped by date)
    public void FetchRequestedProducts(Action<List<RequestGroup>> onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/requested-products";

        StartCoroutine(APIClient.GetJSON(url,
            response =>
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject<List<RequestGroup>>(response);
                    onSuccess?.Invoke(parsed);
                }
                catch (Exception ex)
                {
                    onError?.Invoke("Parsing failed: " + ex.Message);
                }
            },
            error => { Debug.LogError("Fetch request error: " + error); onError?.Invoke(error); }
        ));
    }

    // 5. Mark a specific request as received
    public void MarkRequestReceived(int requestId, Action onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/mark-received/{requestId}";

        StartCoroutine(APIClient.PostJSON(url, "", // No body needed
            response => { Debug.Log("Marked as received"); onSuccess?.Invoke(); },
            error => { Debug.LogError("Mark received error: " + error); onError?.Invoke(error); }
        ));
    }

    public void MarkRequestNotReceived(int requestId, Action onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/mark-not-received/{requestId}";

        StartCoroutine(APIClient.PostJSON(url, "", // No body needed
            response => { Debug.Log("Marked as received"); onSuccess?.Invoke(); },
            error => { Debug.LogError("Mark received error: " + error); onError?.Invoke(error); }
        ));
    }

    // === Data Classes ===

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

    [Serializable]
    public class SimpleProductRequest
    {
        public string product_name;
    }

    [Serializable]
    public class ProductRequest
    {
        public int id;
        public string product_name;
        public bool received;
        public string requested_at;
        public string received_at;
    }

    [Serializable]
    public class RequestGroup
    {
        public string date;
        public List<ProductRequest> requests;
    }
}
