using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mailButton;
    [SerializeField] private Button stockButton;
    [SerializeField] private Button imagesButton;

    [Header("Profile UI")]
    [SerializeField] private TMP_Text profileName;

    private void OnEnable()
    {
        GUIManager.Instance.CheckProfileData();

        mailButton?.onClick.AddListener(OnMailButtonClicked);
        stockButton?.onClick.AddListener(OnStockButtonClicked);
        imagesButton?.onClick.AddListener(OnImagesButtonClicked);
    }

    private void OnDisable()
    {
        mailButton?.onClick.RemoveListener(OnMailButtonClicked);
        stockButton?.onClick.RemoveListener(OnStockButtonClicked);
        imagesButton?.onClick.RemoveListener(OnImagesButtonClicked);
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
        GUIManager.Instance.ShowExcelUploadPopup();
    }
    
    private void OnImagesButtonClicked()
    {
        GUIManager.Instance.ShowImageUploadPopup();
    }
}
