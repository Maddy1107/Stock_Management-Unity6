using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageScreen : UIPage<ImageScreen>
{
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Transform imageContainer;
    [SerializeField] private Button submitButton;

    private string[] currentImagePaths;

    private void OnEnable()
    {
        submitButton?.onClick.AddListener(HandleSubmit);
    }

    private void OnDisable()
    {
        submitButton?.onClick.RemoveListener(HandleSubmit);
    }

    public void Initialize(string[] imagePaths)
    {
        currentImagePaths = imagePaths;
        PopulateImages(imagePaths);
    }

    private void PopulateImages(string[] imagePaths)
    {
        ClearContainer(imageContainer);

        foreach (string path in imagePaths)
        {
            if (string.IsNullOrEmpty(path)) continue;

            GameObject imageGO = Instantiate(imagePrefab, imageContainer);
            if (imageGO.TryGetComponent(out ImageItem item))
            {
                item.SetData(path);
            }
        }
    }

    private void HandleSubmit()
    {
        string month = DateTime.Now.ToString("MMMM");
        string zipFileName = $"Closing_Stock_Images_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

        GUIManager.Instance.ZipFile(currentImagePaths, zipFileName, (zipFilePath) =>
        {
            if (string.IsNullOrEmpty(zipFilePath))
            {
                Debug.LogError("Failed to create zip file.");
                return;
            }

            Debug.Log($"Images zipped successfully: {zipFilePath}");

            GUIManager.Instance.OpenFolder(zipFilePath);
            
            GUIManager.Instance.ShowAndroidToast($"Images zipped successfully: {zipFilePath}");
        });
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
