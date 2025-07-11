using System;
using System.Collections;
using UnityEngine;

public class BackendLoader
{
    public Action<string> OnStatusUpdate;
    public Action<float> OnProgressUpdate;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private const string BaseUrl = "http://127.0.0.1:5000";
#else
    private const string BaseUrl = "https://backendapi-flask.onrender.com";
#endif

    private const float RetryDelay = 1.5f;
    private readonly string envUrl = $"{BaseUrl}/set_env";
    private readonly string warmupUrl = $"{BaseUrl}/warmup";
    private readonly string buildType = GetBuildType();

    public IEnumerator StartBackendSetup()
    {
        float progress = 0f;

        string payload = JsonUtility.ToJson(new BuildTypePayload { build_type = buildType });

        bool setEnvDone = false;
        bool warmupDone = false;

        // Step 1: /set_env
        while (!setEnvDone)
        {
            OnStatusUpdate?.Invoke("Setting environment...");

            yield return APIClient.PostJSON(
                envUrl,
                payload,
                onSuccess: (res) =>
                {
                    Debug.Log("✅ SetEnv OK: " + res);
                    setEnvDone = true;
                    progress = 0.4f;
                    OnProgressUpdate?.Invoke(progress);
                },
                onError: (err) =>
                {
                    Debug.LogWarning("❌ SetEnv failed: " + err);
                });

            if (!setEnvDone)
                yield return new WaitForSeconds(RetryDelay);
        }

        // Step 2: /warmup
        while (!warmupDone)
        {
            OnStatusUpdate?.Invoke("Warming up server...");

            yield return APIClient.GetJSON(
                warmupUrl,
                onSuccess: (res) =>
                {
                    Debug.Log("🔥 Warmup OK: " + res);
                    warmupDone = true;
                    progress = 1f;
                    OnProgressUpdate?.Invoke(progress);
                },
                onError: (err) =>
                {
                    Debug.LogWarning("⚠️ Warmup failed: " + err);
                });

            if (!warmupDone)
                yield return new WaitForSeconds(RetryDelay);
        }
    }

    private static string GetBuildType()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return "development";
#else
        return "production";
#endif
    }

    [Serializable]
    private class BuildTypePayload
    {
        public string build_type;
    }
}
