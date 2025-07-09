using UnityEngine;
using UnityEngine.UI;

public class ImagePreviewPopup : UIPopup<ImagePreviewPopup>
{
    [SerializeField] private Image previewImage;

    public void ShowImage(Sprite sprite)
    {
        if (sprite != null)
        {
            previewImage.sprite = sprite;
            Show();
        }
    }

}
