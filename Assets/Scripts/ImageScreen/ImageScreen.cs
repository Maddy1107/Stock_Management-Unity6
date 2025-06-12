using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageScreen : MonoBehaviour
{
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Transform imageContainer;
    [SerializeField] private Button backButton;
    [SerializeField] private Button submitButton;

    private string[] currentImagePaths;

    private void OnEnable()
    {
        if (backButton != null)
            backButton.onClick.AddListener(HandleBackButton);

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
    }

    private void OnDisable()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(HandleBackButton);

        if (submitButton != null)
            submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
    }

    public void Initialize(string[] imagePaths)
    {
        currentImagePaths = imagePaths;
        LoadImage(imagePaths);
    }

    private void LoadImage(string[] imagePaths)
    {
        ClearChildren(imageContainer);

        foreach (var path in imagePaths)
        {
            if (string.IsNullOrEmpty(path)) continue;

            GameObject imageObject = Instantiate(imagePrefab, imageContainer);
            ImageItem item = imageObject.GetComponent<ImageItem>();
            if (item != null)
            {
                item.SetData(path);
            }
        }
    }

    private void OnSubmitButtonClicked()
    {
        if (currentImagePaths == null || currentImagePaths.Length == 0)
        {
            Debug.LogWarning("No images selected to zip.");
            GUIManager.Instance.ShowAndroidToast("No images selected to zip.");
            return;
        }

        string currMonth = DateTime.Now.ToString("MMMM");
        string zipName = $"Closing_Stock_Images_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        string zipPath = string.Empty;

#if UNITY_EDITOR
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string editorFolder = Path.Combine(desktopPath, "ClosingStock", currMonth);
        Directory.CreateDirectory(editorFolder);
        zipPath = Path.Combine(editorFolder, zipName);
#elif UNITY_ANDROID
        string downloadsPath = "/storage/emulated/0/Download/ClosingStock/" + currMonth;
        if (!Directory.Exists(downloadsPath))
        {
            Directory.CreateDirectory(downloadsPath);
        }
        zipPath = Path.Combine(downloadsPath, zipName);
#endif

        try
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (string path in currentImagePaths)
                {
                    if (!File.Exists(path)) continue;

                    string fileName = Path.GetFileName(path);
                    archive.CreateEntryFromFile(path, fileName);
                }
            }

            Debug.Log("Images zipped successfully at: " + zipPath);
            GUIManager.Instance.ShowAndroidToast("Images zipped successfully.");

#if UNITY_EDITOR
            EditorUtility.RevealInFinder(zipPath);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to zip images: " + e.Message);
            GUIManager.Instance.ShowAndroidToast("Failed to zip images.");
        }
        
        HandleBackButton();
    }

    public void HandleBackButton()
    {
        gameObject.SetActive(false);
        //GUIManager.Instance.ShowMainMenuPanel();
    }

    private void ClearChildren(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }
}
