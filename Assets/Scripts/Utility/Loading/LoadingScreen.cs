using UnityEngine;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        HideInstant();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        Invoke(nameof(Disable), 0.3f); // Let fade out happen (if you animate it)
    }

    public void HideInstant()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }

    public void SetLoadingText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }
}
