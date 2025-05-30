using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;

public class AboutPanel : MonoBehaviour
{
    [Header("UI References")]
    public Button uploadButton;
    public TMP_InputField nameInputField;
    public TMP_Text uploadedImageText;
    public Button submitButton;
    public Button closeButton;

    private Texture2D uploadedTexture;

    private string ImageSavePath => Path.Combine(Application.persistentDataPath, "profile_image.png");
    private const string NameKey = "SavedUserName";

    void Start()
    {
        uploadButton.onClick.AddListener(PickImage);
        submitButton.onClick.AddListener(Submit);
        closeButton.onClick.AddListener(ClosePopup);

        uploadedImageText.text = "No image selected";

        var (savedName, savedTexture) = LoadSavedData();

        if (!string.IsNullOrEmpty(savedName) && savedTexture != null)
        {
            GUIManager.Instance.ShowMainMenuPanel(savedTexture, savedName);
            ClosePopup();
        }
    }

    void PickImage()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadAndDisplayImage(path));
        }
#else
        NativeGallery.GetImageFromGallery(path =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                StartCoroutine(LoadAndDisplayImage(path));
            }
        }, "Select an image", "image/*");
#endif
    }

    IEnumerator LoadAndDisplayImage(string path)
    {
        uploadedImageText.text = Path.GetFileName(path);

        // Load texture with NativeGallery
        Texture2D loadedTexture = NativeGallery.LoadImageAtPath(path, 1024);
        if (loadedTexture == null)
        {
            Debug.LogWarning($"Failed to load image at {path}");
            yield break;
        }

        // Convert non-readable texture to readable
        uploadedTexture = CreateReadableTexture(loadedTexture);

        // Save image as PNG file
        SaveTextureToFile(uploadedTexture);

        yield return null;
    }

    private Texture2D CreateReadableTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readableTex;
    }

    private void SaveTextureToFile(Texture2D texture)
    {
        try
        {
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(ImageSavePath, pngData);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save image to {ImageSavePath}: {ex.Message}");
        }
    }

    public void Submit()
    {
        string userName = nameInputField.text?.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("User name cannot be empty.");
            return;
        }

        if (uploadedTexture == null)
        {
            Debug.LogWarning("Please upload an image before submitting.");
            return;
        }

        PlayerPrefs.SetString(NameKey, userName);
        PlayerPrefs.Save();

        GUIManager.Instance.ShowMainMenuPanel(uploadedTexture, userName);
        ClosePopup();
    }

    public (string, Texture2D) LoadSavedData()
    {
        string savedName = PlayerPrefs.GetString(NameKey, null);
        Texture2D savedTexture = null;

        if (File.Exists(ImageSavePath))
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(ImageSavePath);
                if (imageData != null && imageData.Length > 0)
                {
                    savedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!savedTexture.LoadImage(imageData))
                    {
                        Debug.LogWarning("Failed to load saved image data.");
                        savedTexture = null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading saved image: {ex.Message}");
            }
        }

        return (savedName, savedTexture);
    }

    public void ClosePopup()
    {
        GetComponent<PopupAnimator>()?.Hide();
    }
}
