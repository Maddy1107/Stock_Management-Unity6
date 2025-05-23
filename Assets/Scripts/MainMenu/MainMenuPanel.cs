using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Header("Buttons")]
    public Button mailButton;
    public Button stockButton;

    public Image myImage;
    public TMP_Text myName;

    void OnEnable()
    {
        GUIManager.Instance.CheckProfileData();
        
        if (mailButton != null) mailButton.onClick.AddListener(OnMailButtonClicked);
        if (stockButton != null) stockButton.onClick.AddListener(OnStockButtonClicked);
    }

    void OnDisable()
    {
        if (mailButton != null) mailButton.onClick.RemoveListener(OnMailButtonClicked);
        if (stockButton != null) stockButton.onClick.RemoveListener(OnStockButtonClicked);
    }

    public void Initialize(Texture2D texture, string name)
    {
        if (myImage != null && texture != null)
        {
            myImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        if (myName != null && !string.IsNullOrEmpty(name))
        {
            myName.text = name;
        }
    }

    private void OnMailButtonClicked()
    {
        GUIManager.Instance.ShowSelectMailPopup();
    }

    private void OnStockButtonClicked()
    {
        // TODO: Implement stock panel logic
        Debug.Log("Stock button clicked - not yet implemented.");
    }
}
