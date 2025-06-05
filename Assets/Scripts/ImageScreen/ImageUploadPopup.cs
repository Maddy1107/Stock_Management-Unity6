using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageUploadPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text fileDetailsText;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private string[] imageFilePaths;
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
        NativeFilePicker.PickMultipleFiles((paths) =>
        {
            if (paths == null || paths.Length == 0)
            {
                Debug.Log("File selection cancelled");
                return;
            }

           imageFilePaths = paths;
            Debug.Log("Image files picked: " + string.Join(", ", imageFilePaths));

            HandlePickedExcelFile(imageFilePaths[0]);
        },
        new string[] { ".png", ".jpg", ".jpeg" });
    }

    private void HandlePickedExcelFile(string filePath)
    {
        if (fileDetailsText != null)
        {
            fileDetailsText.text = $"{imageFilePaths.Length} files selected";
        }
    }

    public void OnSubmitButtonClicked()
    {
        if (fileDetailsText != null && 
            !string.IsNullOrEmpty(fileDetailsText.text) && 
            fileDetailsText.text != "No file selected")
        {
            GUIManager.Instance.ShowStockScreen(fileDetailsText.text);
            ClosePopup();
        }
        else
        {
            GUIManager.Instance.ShowAndroidToast("Please select an Excel file first.");
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
