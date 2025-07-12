using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
using UnityEngine.Networking;
#endif

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }
    private List<string> tempPaths = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        MainMenuPanel.Instance?.Show();
    }

    public void CopyToClipboard(string stringToCopy)
    {
        GUIUtility.systemCopyBuffer = stringToCopy.Trim();
        ShowAndroidToast("Copied to clipboard");
    }

    public void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass toastClass = new("android.widget.Toast");
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                toast.Call("show");
            }));
        }
#else
        Debug.Log("Toast: " + message);
#endif
    }

    public string SaveFile(string filename, string month, byte[] data, string mimeType = "application/octet-stream")
    {
        string persistentPath = $"{Application.persistentDataPath}/stockdata/{filename}";

        try
        {
            File.WriteAllBytes(persistentPath, data);
            tempPaths.Add(persistentPath);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to write to persistentDataPath: " + e.Message);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
                AndroidJavaObject values = new AndroidJavaObject("android.content.ContentValues");

                values.Call<AndroidJavaObject>("put", "relative_path", $"Download/ClosingStock/{month}/");
                values.Call<AndroidJavaObject>("put", "title", filename);
                values.Call<AndroidJavaObject>("put", "display_name", filename);
                values.Call<AndroidJavaObject>("put", "mime_type", mimeType);
                values.Call<AndroidJavaObject>("put", "is_pending", 1);

                AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Downloads");
                AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", mediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"), values);

                if (uri == null)
                {
                    Debug.LogError("❌ Failed to insert into MediaStore.");
                    return persistentPath;
                }

                AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
                using (AndroidJavaObject bufferedStream = new AndroidJavaObject("java.io.BufferedOutputStream", outputStream))
                {
                    bufferedStream.Call("write", data);
                    bufferedStream.Call("flush");
                    bufferedStream.Call("close");
                }

                values.Call<AndroidJavaObject>("put", "is_pending", 0);
                contentResolver.Call<int>("update", uri, values, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("⚠️ MediaStore save failed: " + e.Message);
        }
#endif

        return persistentPath;
    }

    public byte[] GenerateZipBytes(string[] filePaths)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (string filePath in filePaths)
                {
                    if (!File.Exists(filePath))
                        continue;

                    string fileName = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(fileName);

                    using (var entryStream = entry.Open())
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
            return memoryStream.ToArray();
        }
    }

    public void ShareFilesOrJustText(string subject, string message)
    {
        NativeShare share = new NativeShare()
            .SetSubject(subject)
            .SetText(message);

        if (tempPaths != null && tempPaths.Count > 0)
        {
            foreach (var path in tempPaths)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    share.AddFile(path);
                }
            }
        }

        share.Share();
    }


    private void OnApplicationQuit()
    {
        foreach (string path in tempPaths)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning("⚠️ Failed to delete: " + path + " Error: " + e.Message);
            }
        }
    }
}
