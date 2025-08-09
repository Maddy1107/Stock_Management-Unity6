using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiConfig
{
    public static string DevBaseUrl = "https://stock-dev-1ac3.onrender.com";
    public static string ProdBaseUrl = "https://backendapi-flask.onrender.com";
    public static string EditorBaseUrl = "http://127.0.0.1:5001";
}

public static class APIClient
{
    private const int DefaultTimeout = 10;
    private const int MaxRetries = 1; // Number of retries on internal server error

    public static IEnumerator PostJSON(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        int attempt = 0;

        while (attempt <= MaxRetries)
        {
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = DefaultTimeout;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(www.downloadHandler.text);
                    yield break;
                }

                bool isInternalServerError = www.responseCode == 500;

                if (!isInternalServerError || attempt == MaxRetries)
                {
                    onError?.Invoke(www.error + " | " + www.downloadHandler.text);
                    yield break;
                }

                attempt++;
                Debug.LogWarning($"Internal server error. Retrying POST JSON to {url} (Attempt {attempt})...");
                yield return new WaitForSeconds(1); // optional delay before retry
            }
        }
    }

    public static IEnumerator GetJSON(string url, Action<string> onSuccess, Action<string> onError)
    {
        int attempt = 0;

        while (attempt <= MaxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Accept", "application/json");
                www.timeout = DefaultTimeout;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(www.downloadHandler.text);
                    yield break;
                }

                bool isInternalServerError = www.responseCode == 500;

                if (!isInternalServerError || attempt == MaxRetries)
                {
                    onError?.Invoke(www.error + " | " + www.downloadHandler.text);
                    yield break;
                }

                attempt++;
                Debug.LogWarning($"Internal server error. Retrying GET JSON from {url} (Attempt {attempt})...");
                yield return new WaitForSeconds(1);
            }
        }
    }

    public static IEnumerator PostForm(string url, WWWForm form, Action<byte[]> onSuccess, Action<string> onError)
    {
        int attempt = 0;

        while (attempt <= MaxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                www.timeout = DefaultTimeout;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(www.downloadHandler.data);
                    yield break;
                }

                bool isInternalServerError = www.responseCode == 500;

                if (!isInternalServerError || attempt == MaxRetries)
                {
                    onError?.Invoke(www.error + " | " + www.downloadHandler.text);
                    yield break;
                }

                attempt++;
                Debug.LogWarning($"Internal server error. Retrying POST form to {url} (Attempt {attempt})...");
                yield return new WaitForSeconds(1);
            }
        }
    }
}
