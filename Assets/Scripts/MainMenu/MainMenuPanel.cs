using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mailButton;
    [SerializeField] private Button stockButton;

    [Header("Profile UI")]
    [SerializeField] private TMP_Text profileName;

    private void OnEnable()
    {
        GUIManager.Instance.CheckProfileData();

        mailButton?.onClick.AddListener(OnMailButtonClicked);
        stockButton?.onClick.AddListener(OnStockButtonClicked);
    }

    private void OnDisable()
    {
        mailButton?.onClick.RemoveListener(OnMailButtonClicked);
        stockButton?.onClick.RemoveListener(OnStockButtonClicked);
    }

    public void Initialize(string name)
    {
        if (!string.IsNullOrWhiteSpace(name) && profileName)
        {
            profileName.text = name;
        }
    }

    private void OnMailButtonClicked()
    {
        GUIManager.Instance.ShowSelectMailPopup();
    }

    private void OnStockButtonClicked()
    {
        Debug.Log("Stock button clicked - feature not yet implemented.");
    }
}
