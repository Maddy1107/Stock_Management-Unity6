using UnityEngine;
using UnityEngine.UI;

public abstract class UIPage<T> : UIPageBase where T : UIPage<T>
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

    protected virtual void Awake()
    {
        if (_instance == null)
            _instance = this as T;
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
    }

    protected virtual void OnBackPressed()
    {
        Hide();
        MainMenuPanel.Instance?.Show();
    }

    // Optional override to maintain consistent behavior
    public override void Hide()
    {
        base.Hide();
    }
}
