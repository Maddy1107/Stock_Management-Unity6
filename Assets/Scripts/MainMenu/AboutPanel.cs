using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AboutPanel : UIPopup<AboutPanel>
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button submitButton;
    private bool hasCheckedProfileData = false;
    private const string NameKey = "SavedUserName";

    protected override void Awake()
    {
        base.Awake();
        CheckProfileData();
    }

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
        MainMenuPanel.Instance?.SetProfileName(LoadSavedData());
        Hide();
    }

    public string LoadSavedData()
    {
        return PlayerPrefs.GetString(NameKey, null);
    }

    public void CheckProfileData()
    {
        if (hasCheckedProfileData) return;

        hasCheckedProfileData = true;

        string name = LoadSavedData();

        if (string.IsNullOrWhiteSpace(name))
        {
            Show();
        }
    }
}
