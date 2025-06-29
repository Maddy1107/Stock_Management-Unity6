using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class APIClient
{
    public static IEnumerator PostJSON(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(www.downloadHandler.text);
            else
                onError?.Invoke(www.error + " | " + www.downloadHandler.text);
        }
    }

    public static IEnumerator GetJSON(string url, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Accept", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(www.downloadHandler.text);
            else
                onError?.Invoke(www.error + " | " + www.downloadHandler.text);
        }
    }

    public static IEnumerator PostForm(string url, WWWForm form, Action<byte[]> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(www.downloadHandler.data);
            else
                onError?.Invoke(www.error + " | " + www.downloadHandler.text);
        }
    }
}
