using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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

    public void ZipFile(string[] filepaths, string filename, Action<string> onSuccess = null, Action<Exception> onError = null)
    {
        string zipPath = GetZipFilePath(out bool allowOverwrite, filename);

        if (string.IsNullOrEmpty(zipPath))
        {
            ShowAndroidToast("Cancelled or invalid folder.");
            onError?.Invoke(new Exception("Cancelled or invalid folder."));
            return;
        }

        if (File.Exists(zipPath) && !allowOverwrite)
        {
            ShowAndroidToast("Zipping cancelled.");
            onError?.Invoke(new Exception("Zipping cancelled."));
            return;
        }

        try
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using FileStream zipStream = new(zipPath, FileMode.Create);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);

            foreach (string path in filepaths)
            {
                if (!File.Exists(path)) continue;
                archive.CreateEntryFromFile(path, Path.GetFileName(path));
            }

            ShowAndroidToast("Images zipped successfully.");
            Debug.Log($"Images zipped at: {zipPath}");

#if UNITY_EDITOR
            EditorUtility.RevealInFinder(zipPath);
#endif
            onSuccess?.Invoke(zipPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to zip images: " + ex.Message);
            ShowAndroidToast("Failed to zip images.");
            onError?.Invoke(ex);
        }
    }

    private string GetZipFilePath(out bool allowOverwrite, string zipFileName)
    {

#if UNITY_EDITOR
        allowOverwrite = true;

        string selectedFolder = EditorUtility.SaveFolderPanel("Select Folder to Save ZIP", "", "");
        if (string.IsNullOrEmpty(selectedFolder))
        {
            return null;
        }

        string zipPath = Path.Combine(selectedFolder, zipFileName);
        if (File.Exists(zipPath))
        {
            bool confirmed = EditorUtility.DisplayDialog("File Exists",
                $"A file named \"{zipFileName}\" already exists.\nDo you want to overwrite it?",
                "Yes", "No");

            allowOverwrite = confirmed;
        }

        return zipPath;

#elif UNITY_ANDROID
        string month = System.DateTime.Now.ToString("MMMM");
        string folderPath = Path.Combine("/storage/emulated/0/Download/ClosingStock", month);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string zipPath = Path.Combine(folderPath, zipFileName);

        allowOverwrite = true; // No popup — just overwrite
        return zipPath;
#else
        allowOverwrite = false;
        return null;
#endif
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


#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void OnGUI()
    {
        const int buttonWidth = 400;
        const int buttonHeight = 100;

        // Bottom-right corner
        Rect buttonRect = new Rect(
            Screen.width - buttonWidth - 200,
            Screen.height - buttonHeight - 200,
            buttonWidth,
            buttonHeight
        );

        if (GUI.Button(buttonRect, "Open Outlook"))
        {
            OpenEmail(
    "Feedback for your app",
    "Hey there!\n\nI just wanted to say..."
);

        }
    }
#endif
}
