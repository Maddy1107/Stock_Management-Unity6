using UnityEngine;
using UnityEngine.UI;

public class ImagePreviewPopup : UIPopup<ImagePreviewPopup>
{
    [SerializeField] private Image previewImage;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform imageContainer;

    public void ShowImage(Sprite sprite)
    {
        if (sprite != null)
        {
            previewImage.sprite = sprite;
            Show();
        }
    }

}
