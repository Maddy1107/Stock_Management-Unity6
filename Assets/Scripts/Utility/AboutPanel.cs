using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;

public class AboutPanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button submitButton;
    public Button closeButton;
    private const string NameKey = "SavedUserName";

    void Start()
    {
        submitButton.onClick.AddListener(Submit);
        closeButton.onClick.AddListener(ClosePopup);

        string savedName = LoadSavedData();

        if (!string.IsNullOrEmpty(savedName))
        {
            GUIManager.Instance.ShowMainMenuPanel(savedName);
            ClosePopup();
        }
    }

    public void Submit()
    {
        string userName = nameInputField.text?.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("User name cannot be empty.");
            return;
        }


        PlayerPrefs.SetString(NameKey, userName);
        PlayerPrefs.Save();

        GUIManager.Instance.ShowMainMenuPanel(userName);
        ClosePopup();
    }

    public string LoadSavedData()
    {
        string savedName = PlayerPrefs.GetString(NameKey, null);

        return savedName;
    }

    public void ClosePopup()
    {
        GetComponent<PopupAnimator>()?.Hide();
    }
}
