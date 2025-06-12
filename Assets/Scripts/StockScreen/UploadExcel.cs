using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UploadExcel : MonoBehaviour
{
    [SerializeField] private TMP_Text excelFilePath;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;

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
        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("File selection cancelled");
                return;
            }

            Debug.Log("Excel file picked at: " + path);

            HandlePickedExcelFile(path);
        },
        new string[] { ".xlsx", ".xls" });
    }

    private void HandlePickedExcelFile(string filePath)
    {
        if (excelFilePath != null)
        {
            excelFilePath.text = filePath;
        }
    }

    public void OnSubmitButtonClicked()
    {
        if (excelFilePath != null && 
            !string.IsNullOrEmpty(excelFilePath.text) && 
            excelFilePath.text != "No file selected")
        {
            //GUIManager.Instance.ShowStockScreen(excelFilePath.text);
            ClosePopup();
        }
        else
        {
            GUIManager.Instance.ShowAndroidToast("Please select an Excel file first.");
        }
    }

    public void ResetUI()
    {
        if (excelFilePath != null)
        {
            excelFilePath.text = "No file selected";
        }
    }
    
    private void ClosePopup()
    {
        ResetUI();
        GetComponent<PopupAnimator>()?.Hide();
    }
}
