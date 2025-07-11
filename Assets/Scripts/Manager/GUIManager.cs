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

                AndroidJavaObject mediaStore = new AndroidJavaClass("android.provider.MediaStore$Downloads");
                AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", mediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"), values);

                if (uri == null)
                {
                    Debug.LogError("Failed to get MediaStore URI.");
                    return null;
                }

                // Write to output stream
                AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
                IntPtr streamPtr = outputStream.GetRawObject();

                using (AndroidJavaObject bufferStream = new AndroidJavaObject("java.io.BufferedOutputStream", outputStream))
                {
                    bufferStream.Call("write", data);
                    bufferStream.Call("flush");
                    bufferStream.Call("close");
                }

                values.Call<AndroidJavaObject>("put", "is_pending", 0);
                contentResolver.Call<int>("update", uri, values, null, null);

                return uri.ToString(); // Can be returned for reference
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving file on Android: " + ex.Message);
            return null;
        }
#else
        // For Editor/PC
        try
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"ClosingStock/{month}");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, filename);
            File.WriteAllBytes(path, data);
            Debug.Log("Saved file to: " + path);
            return path;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving file in Editor: " + ex.Message);
            return null;
        }
#endif
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

    public void OpenEmail(string subject, string body)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                string uriString = $"mailto:?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
                AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", uriString);

                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.SENDTO", uri);
                intent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK

                activity.Call("startActivity", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open email app: " + e.Message);
            GUIManager.Instance.ShowAndroidToast("No email app found or failed to open.");
        }
#else
        Debug.Log("This only works on a real Android device.");
#endif
    }
}
