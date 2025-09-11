using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DonePopup : UIPopup<DonePopup>
{
    [SerializeField] private Image bgPanel;
    [SerializeField] private Sprite successBG;
    [SerializeField] private Sprite failedBG;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text body;
    [SerializeField] private Button okButton;

    private bool isSuccess;

    void OnEnable()
    {
        okButton.onClick.AddListener(OnOkButtonPressed);
    }

    void OnDisable()
    {
        okButton.onClick.RemoveAllListeners();
    }

    public void Initialize(string titleTxt, bool success)
    {
        isSuccess = success;
        body.text = titleTxt;

        bgPanel.sprite = success ? successBG : failedBG;
        title.text = success ? "SUCCESS" : "FAILED";

        Show();
    }

    public void OnOkButtonPressed()
    {
        // if (isSuccess)
        //     MainMenuPanel.Instance.Show();
        Hide();
    }
}
