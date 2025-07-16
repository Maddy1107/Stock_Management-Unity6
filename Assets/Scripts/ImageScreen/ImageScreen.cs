using System;
using System.Collections.Generic;
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
        LoadingScreen.Instance.Show();

        string month = DateTime.Now.ToString("MMMM");

        string zipFileName = $"Closing_Stock_Images_{month}.zip";

        byte[] zipBytes = GUIManager.Instance.GenerateZipBytes(currentImagePaths);

        string path = GUIManager.Instance.SaveFile(zipFileName, month, zipBytes, "application/zip");

        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("ZIP saved at: " + path);
            DonePopup.Instance.Initialize("ZIP saved Successfully", true);
            GUIManager.Instance.ShowAndroidToast("ZIP saved successfully.");
        }
        else
        {
            DonePopup.Instance.Initialize("Failed to save ZIP.", false);
            GUIManager.Instance.ShowAndroidToast("Failed to save ZIP.");
        }

        LoadingScreen.Instance.Hide();
    }


    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
