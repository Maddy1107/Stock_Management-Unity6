using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AboutPanel : UIPopup<AboutPanel>
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button submitButton;

    private const string NameKey = "SavedUserName";

    private void OnEnable()
    {
        submitButton.onClick.AddListener(Submit);
    }

    private void OnDisable()
    {
        submitButton.onClick.RemoveListener(Submit);
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
