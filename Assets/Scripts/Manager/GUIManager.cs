using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

    private readonly List<string> tempPaths = new();
    private string FolderPath => Path.Combine(Application.persistentDataPath, "stockdata");

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
        DeleteAllFilesInFolder(FolderPath);
    }

    private void OnApplicationQuit() => DeleteAllFilesInFolder(FolderPath);

    // 🔹 Toast
    public void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            using AndroidJavaClass toastClass = new("android.widget.Toast");
            AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
            toast.Call("show");
        }));
#else
        Debug.Log("Toast: " + message);
#endif
    }

    // 🔹 Clipboard
    public void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text.Trim();
        ShowAndroidToast("Copied to clipboard");
    }

    // 🔹 Save file to persistent path + MediaStore
    public string SaveFile(string filename, string month, byte[] data, string mimeType = "application/octet-stream")
    {
        string persistentPath = Path.Combine(FolderPath, filename);

        try
        {
            Directory.CreateDirectory(FolderPath);
            File.WriteAllBytes(persistentPath, data);
            tempPaths.Add(persistentPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Write failed: " + e.Message);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaObject values = new("android.content.ContentValues");

        values.Call<AndroidJavaObject>("put", "relative_path", $"Download/ClosingStock/{month}/");
        values.Call<AndroidJavaObject>("put", "title", filename);
        values.Call<AndroidJavaObject>("put", "display_name", filename);
        values.Call<AndroidJavaObject>("put", "mime_type", mimeType);
        values.Call<AndroidJavaObject>("put", "is_pending", new AndroidJavaObject("java.lang.Integer", 1));

        AndroidJavaClass mediaStore = new("android.provider.MediaStore$Downloads");
        AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>(
            "insert", mediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"), values);

        if (uri == null)
        {
            Debug.LogError("MediaStore URI failed.");
            return persistentPath;
        }

        using AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
        using AndroidJavaObject bufferStream = new("java.io.BufferedOutputStream", outputStream))
        {
            bufferStream.Call("write", data);
            bufferStream.Call("flush");
            bufferStream.Call("close");
        }

        // Mark as no longer pending
        values.Call<AndroidJavaObject>("put", "is_pending", new AndroidJavaObject("java.lang.Integer", 0));
        contentResolver.Call<int>("update", uri, values, null, null);
    }
    catch (Exception e)
    {
        Debug.LogError("MediaStore error: " + e.Message);
    }
#endif
        return persistentPath;
    }

    // 🔹 ZIP generation
    public byte[] GenerateZipBytes(string[] filePaths)
    {
        using MemoryStream memoryStream = new();
        using ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true);

        foreach (string file in filePaths)
        {
            if (!File.Exists(file)) continue;
            var entry = archive.CreateEntry(Path.GetFileName(file));
            using var entryStream = entry.Open();
            using var fileStream = File.OpenRead(file);
            fileStream.CopyTo(entryStream);
        }

        return memoryStream.ToArray();
    }

    // 🔹 Validate and Share
    public void CheckIfValidMail(string subject, string message)
    {
        if (CheckTempFilesExist())
            ShareFilesOrJustText(subject, message);
    }

    public bool CheckTempFilesExist()
    {
        if (!Directory.Exists(FolderPath))
        {
            ShowAndroidToast("No export folder found.");
            return false;
        }

        bool hasXlsx = false, hasZip = false;

        foreach (string file in Directory.GetFiles(FolderPath))
        {
            if (file.EndsWith(".xlsx")) hasXlsx = true;
            if (file.EndsWith(".zip")) hasZip = true;
        }

        if (!hasXlsx && !hasZip) ShowAndroidToast("Missing both files.");
        else if (!hasXlsx) ShowAndroidToast("Missing Closing stock file.");
        else if (!hasZip) ShowAndroidToast("Missing Zip file.");

        return hasXlsx && hasZip;
    }

    // 🔹 NativeShare
    public void ShareFilesOrJustText(string subject, string message)
    {
        NativeShare share = new NativeShare()
            .SetSubject(subject)
            .SetText(message);

        foreach (string path in tempPaths)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                share.AddFile(path);
        }

        share.Share();
    }

    // 🔹 Cleanup
    public static void DeleteAllFilesInFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        try
        {
            foreach (string file in Directory.GetFiles(folderPath))
                File.Delete(file);
        }
        catch (Exception e)
        {
            Debug.LogError("Deletion error: " + e.Message);
        }
    }
}
