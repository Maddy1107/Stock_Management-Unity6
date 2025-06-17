using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageUploadPopup : UIPopup<ImageUploadPopup>
{
    [Header("UI References")]
    [SerializeField] private TMP_Text fileDetailsText;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button submitButton;

    private string[] imageFilePaths;

    private void OnEnable()
    {
        ResetUI();
        uploadButton?.onClick.AddListener(OnUploadClicked);
        submitButton?.onClick.AddListener(OnSubmitClicked);
    }

    private void OnDisable()
    {
        uploadButton?.onClick.RemoveListener(OnUploadClicked);
        submitButton?.onClick.RemoveAllListeners();
    }

    private void OnUploadClicked()
    {
#if UNITY_EDITOR
        StartCoroutine(HandleEditorImagePicker());

#elif UNITY_ANDROID
    LoadingManager.ShowWhile(async () =>
    {
        bool callbackReceived = false;

        NativeGallery.GetImagesFromGallery((paths) =>
        {
            if (paths == null || paths.Length == 0)
            {
                Debug.Log("Image selection canceled.");
                callbackReceived = true;
                return;
            }

            imageFilePaths = paths;
            Debug.Log("Images selected: " + string.Join(", ", paths));
            OnImagesPicked();

            callbackReceived = true;
        }, "Select images", "image/*");

        // Wait until callback is hit (you may add timeout here)
        while (!callbackReceived)
            await Task.Yield();
    });

#else
    Debug.LogWarning("Image picking is not supported on this platform.");
#endif
    }

    private IEnumerator HandleEditorImagePicker()
    {
        LoadingScreen.Instance?.Show();
        yield return null; // Let frame render the loading screen

        string folderPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Image Folder", "", "");

        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.Log("Image folder selection canceled.");
            LoadingScreen.Instance?.Hide();
            yield break;
        }

        imageFilePaths = Directory.GetFiles(folderPath)
            .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
            .ToArray();

        if (imageFilePaths.Length == 0)
        {
            Debug.Log("No valid image files in folder.");
            GUIManager.Instance.ShowAndroidToast("No image files found.");
            LoadingScreen.Instance?.Hide();
            yield break;
        }

        Debug.Log("Images selected (Editor): " + string.Join(", ", imageFilePaths));
        OnImagesPicked();
        LoadingScreen.Instance?.Hide();
    }


    private void OnImagesPicked()
    {
        if (fileDetailsText != null)
        {
            fileDetailsText.text = $"{imageFilePaths.Length} file(s) selected";
        }
    }

    private void OnSubmitClicked()
    {
        if (imageFilePaths != null && imageFilePaths.Length > 0)
        {
            ImageScreen.Instance.Show();
            ImageScreen.Instance.Initialize(imageFilePaths);

            ClosePopup();
        }
        else
        {
            GUIManager.Instance.ShowAndroidToast("Please select images first.");
        }
    }

    private void ClosePopup()
    {
        ResetUI();
        GetComponent<PopupAnimator>()?.Hide();
    }

    public void ResetUI()
    {
        fileDetailsText.text = "No file selected";
        imageFilePaths = null;
    }
}
