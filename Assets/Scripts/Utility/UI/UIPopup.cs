using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupAnimator))]
public abstract class UIPopup<T> : UIPopupBase where T : UIPopup<T>
{
    [SerializeField] private Button closeButton;
    private PopupAnimator animator;

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                    Debug.LogError($"Instance of {typeof(T)} not found!");
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _instance = this as T;
        Initialize();
    }

    private void Initialize()
    {
        animator = GetComponent<PopupAnimator>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        else
            Debug.LogWarning($"{name} has no Close button assigned.");
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        animator?.Show();
        OnShow();
    }

    public override void Hide()
    {
        animator?.Hide();
        OnHide();
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
