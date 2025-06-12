using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AboutPanel : Popup<AboutPanel>
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button submitButton;
    public Button closeButton;

    private const string NameKey = "SavedUserName";

    private void Start()
    {
        submitButton.onClick.AddListener(Submit);
        closeButton.onClick.AddListener(Hide); // Use base Hide from Popup<T>

        string savedName = LoadSavedData();

        if (!string.IsNullOrEmpty(savedName))
        {
            Hide();
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

        Hide();
    }

    public string LoadSavedData()
    {
        return PlayerPrefs.GetString(NameKey, null);
    }
}
