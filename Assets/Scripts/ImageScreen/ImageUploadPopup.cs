using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageUploadPopup : Popup<ImageUploadPopup>
{
    [SerializeField] private TMP_Text fileDetailsText;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    private string[] imageFilePaths;
    
    private void OnEnable()
    {
        ResetUI();
        if (uploadButton != null)
        {
            uploadButton.onClick.AddListener(HandleUploadButtonClicked);
        }
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    private void OnDisable()
    {
        if (uploadButton != null)
        {
            uploadButton.onClick.RemoveListener(HandleUploadButtonClicked);
        }
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
        }
    }

    private void HandleUploadButtonClicked()
    {
        #if UNITY_EDITOR
        string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Images", "", "");
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.Log("Folder selection cancelled in Editor");
            return;
        }

        string[] paths = Directory.GetFiles(folderPath, "*.*")
                                .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                                .ToArray();

        if (paths.Length == 0)
        {
            Debug.Log("No image files found in folder.");
            return;
        }

        Debug.Log("Image files picked (Editor): " + string.Join(", ", paths));

        imageFilePaths = paths;
        HandlePickedImage();
        

    #elif UNITY_ANDROID
        NativeFilePicker.PickMultipleFiles((string[] paths) =>
        {
            if (paths == null || paths.Length == 0)
            {
                Debug.Log("File selection cancelled on Android");
                return;
            }

            Debug.Log("Image files picked (Android): " + string.Join(", ", paths));

            imageFilePaths = paths;
            HandlePickedImage();

        }, new string[] { ".png", ".jpg", ".jpeg" });

    #else
        Debug.LogWarning("File picking not supported on this platform.");
    #endif
    }

    private void HandlePickedImage()
    {
        if (fileDetailsText != null)
        {
            fileDetailsText.text = $"{imageFilePaths.Length} files selected";
        }
    }

    public void OnSubmitButtonClicked()
    {
        if (imageFilePaths != null && imageFilePaths.Length > 0)
        {
            //GUIManager.Instance.ShowImageScreen(imageFilePaths);
            ClosePopup();
        }
        else
        {
            GUIManager.Instance.ShowAndroidToast("Please select an image first.");
        }
    }

    public void ResetUI()
    {
        if (fileDetailsText != null)
        {
            fileDetailsText.text = "No file selected";
        }
    }
    
    private void ClosePopup()
    {
        ResetUI();
        GetComponent<PopupAnimator>()?.Hide();
    }
}
