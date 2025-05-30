using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mailButton;
    [SerializeField] private Button stockButton;

    [Header("Profile UI")]
    [SerializeField] private Image profileImage;
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

    public void Initialize(Texture2D texture, string name)
    {
        if (texture && profileImage)
        {
            profileImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

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
