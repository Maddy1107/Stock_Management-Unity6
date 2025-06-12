using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageItem : MonoBehaviour
{

    [SerializeField] private Button previewButton;
    private Sprite imageSprite;

    public void OnEnable()
    {
        previewButton = GetComponentInChildren<Button>();

        if (previewButton != null)
        {
            previewButton.onClick.AddListener(HandlePreviewButtonClicked);
        }
    }

    private void HandlePreviewButtonClicked()
    {
        //GUIManager.Instance.ShowImagePreviewPopup(imageSprite);
    }

    public void OnDisable()
    {
        if (previewButton != null)
        {
            previewButton.onClick.RemoveListener(HandlePreviewButtonClicked);
        }
    }
    public void SetData(string imagePath)
    {
        // Assuming you have an Image component to display the image
        var imageComponent = GetComponent<Image>();
        if (imageComponent != null && !string.IsNullOrEmpty(imagePath))
        {
            var texture = LoadTexture(imagePath);
            if (texture != null)
            {
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                imageSprite = imageComponent.sprite;
            }
        }
    }

    private Texture2D LoadTexture(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
            return null;

        byte[] fileData = System.IO.File.ReadAllBytes(imagePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            return tex;
        }
        return null;
    }
}
