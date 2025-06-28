using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIPage<T> : UIPageBase, IUIStackElement where T : UIPage<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
            return _instance;
        }
    }

    [SerializeField] private Button backButton;

    [Header("Slide Animation")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private SlideDirection slideInFrom = SlideDirection.Right;
    [SerializeField] private SlideDirection slideOutTo = SlideDirection.Right;
    [SerializeField] private float slideOffset = 1920f; // Canvas width or height

    private RectTransform rectTransform;

    protected virtual void Awake()
    {
        if (_instance == null)
            _instance = this as T;
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        HookBackButton();
    }

    private void HookBackButton()
    {
        if (backButton == null)
        {
            var found = transform.Find("Back") ?? transform.Find("btnBack");
            if (found != null)
                backButton = found.GetComponent<Button>();
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
        else
            Debug.LogWarning($"{name} has no Back button assigned or found.");
    }

    public virtual void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        SetCurrentPage(this);
        StopAllCoroutines();
        StartCoroutine(SlideIn());
        UIStackManager.Instance?.Push(this);

    }

    public override void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(SlideOut());
        UIStackManager.Instance?.Pop(this);
    }

    protected virtual void OnBackPressed()
    {
        Hide();
        MainMenuPanel.Instance?.Show();
    }

    private IEnumerator SlideIn()
    {
        Vector2 start = GetOffsetPosition(slideInFrom);
        Vector2 end = Vector2.zero;

        rectTransform.anchoredPosition = start;
        float time = 0f;

        while (time < slideDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, time / slideDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = end;
    }

    private IEnumerator SlideOut()
    {
        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = GetOffsetPosition(slideOutTo);

        float time = 0f;
        while (time < slideDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, time / slideDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = end;
        gameObject.SetActive(false);
    }

    private Vector2 GetOffsetPosition(SlideDirection direction)
    {
        switch (direction)
        {
            case SlideDirection.Left:
                return new Vector2(-slideOffset, 0);
            case SlideDirection.Right:
                return new Vector2(slideOffset, 0);
            case SlideDirection.Top:
                return new Vector2(0, slideOffset);
            case SlideDirection.Bottom:
                return new Vector2(0, -slideOffset);
            default:
                return Vector2.zero;
        }
    }

    public virtual void OnBack()
    {
        OnBackPressed(); // Default back action
    }
}

public enum SlideDirection
{
    Left,
    Right,
    Top,
    Bottom
}
