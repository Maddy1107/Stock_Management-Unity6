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
    public Button submitButton, closeButton;

    private Texture2D uploadedTexture;

    private string imageSavePath => Path.Combine(Application.persistentDataPath, "profile_image.png");
    private const string nameKey = "SavedUserName";

    void Start()
    {
        uploadButton.onClick.AddListener(PickImage);
        submitButton.onClick.AddListener(Submit);
        closeButton.onClick.AddListener(ClosePopup);
        uploadedImageText.text = "No image selected";

        // Attempt to load and use saved data
        var (name, texture) = LoadSavedData();

        if (!string.IsNullOrEmpty(name) && texture != null)
        {
            GUIManager.Instance.ShowMainMenuPanel(texture, name);
            gameObject.SetActive(false);
        }
    }

    void PickImage()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            uploadedImageText.text = Path.GetFileName(path);
            StartCoroutine(LoadImage(path));
        }
#else
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                uploadedImageText.text = Path.GetFileName(path);
                StartCoroutine(LoadImage(path));
            }
        }, "Select an image", "image/*");
#endif
    }

    IEnumerator LoadImage(string path)
    {
        Texture2D nonReadableTexture = NativeGallery.LoadImageAtPath(path, 1024);
        if (nonReadableTexture == null)
        {
            Debug.LogWarning("Couldn't load texture from " + path);
            yield break;
        }

        // Create a new readable texture using RenderTexture
        Texture2D readableTexture = new Texture2D(nonReadableTexture.width, nonReadableTexture.height, TextureFormat.RGBA32, false);

        RenderTexture tmp = RenderTexture.GetTemporary(
            nonReadableTexture.width,
            nonReadableTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(nonReadableTexture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        uploadedTexture = readableTexture;

        try
        {
            byte[] pngData = uploadedTexture.EncodeToPNG();
            File.WriteAllBytes(imageSavePath, pngData);
            uploadedImageText.text = Path.GetFileName(path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save image: " + ex.Message);
        }
    }



    public void Submit()
    {
        string userName = nameInputField.text?.Trim();

        if (string.IsNullOrEmpty(userName) || uploadedTexture == null)
        {
            Debug.LogWarning("Name and image are required before submission.");
            return;
        }

        PlayerPrefs.SetString(nameKey, userName);
        PlayerPrefs.Save();

        GUIManager.Instance.ShowMainMenuPanel(uploadedTexture, userName);
        gameObject.SetActive(false);
    }

    public (string, Texture2D) LoadSavedData()
    {
        string name = PlayerPrefs.GetString(nameKey, null);
        Texture2D texture = null;

        if (File.Exists(imageSavePath))
        {
            try
            {
                byte[] data = File.ReadAllBytes(imageSavePath);
                if (data != null && data.Length > 0)
                {
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(data))
                    {
                        Debug.LogWarning("Failed to decode saved image.");
                        texture = null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error reading saved image: " + ex.Message);
            }
        }

        return (name, texture);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
