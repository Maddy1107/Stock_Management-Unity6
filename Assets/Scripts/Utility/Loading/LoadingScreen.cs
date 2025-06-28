using UnityEngine;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    private CanvasGroup canvasGroup;
    public GameObject icon;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        canvasGroup = GetComponent<CanvasGroup>();
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

    void Update()
    {
        if (enabled && icon != null)
        {
            icon.transform.Rotate(0f, 0f, -180f * Time.deltaTime);
        }
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
