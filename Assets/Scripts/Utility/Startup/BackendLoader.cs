using System;
using System.Collections;
using UnityEngine;

public class BackendLoader
{
    public Action<string> OnStatusUpdate;
    public Action<float> OnProgressUpdate;

#if UNITY_EDITOR
    private string BaseUrl = ApiConfig.EditorBaseUrl;
#elif DEVELOPMENT_BUILD
    private string BaseUrl = ApiConfig.DevBaseUrl;
#else
    private string BaseUrl = ApiConfig.ProdBaseUrl;
#endif

    private readonly float RetryDelay = 1.5f;

    public IEnumerator StartBackendSetup()
    {
        float progress = 0f;
        bool warmupDone = false;
        string warmupUrl = $"{BaseUrl}/warmup";

        while (!warmupDone)
        {
            OnStatusUpdate?.Invoke("Warming up server...");

            yield return APIClient.GetJSON(
                warmupUrl,
                onSuccess: (res) =>
                {
                    Debug.Log("Warmup OK: " + res);
                    warmupDone = true;
                    progress = 1f;
                    OnProgressUpdate?.Invoke(progress);
                },
                onError: (err) =>
                {
                    Debug.LogWarning("Warmup failed: " + err);
                });

            if (!warmupDone)
                yield return new WaitForSeconds(RetryDelay);
        }
    }
}
