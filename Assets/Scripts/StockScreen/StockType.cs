using System;
using UnityEngine;
using UnityEngine.UI;

public class StockType : UIPopup<StockType>
{
    [SerializeField] public Button uploadButton;
    [SerializeField] public Button generateButton;

    void OnEnable()
    {
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
    }

    private void OnGenerateButtonClicked()
    {
        throw new NotImplementedException();
    }

    private void OnUploadButtonClicked()
    {
        throw new NotImplementedException();
    }

    void OnDisable()
    {
        uploadButton.onClick.RemoveListener(OnUploadButtonClicked);
        generateButton.onClick.RemoveListener(OnGenerateButtonClicked);
    }
}
