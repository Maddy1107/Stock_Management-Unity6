using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupAnimator))]
public abstract class UIPopup<T> : UIPopupBase, IUIStackElement where T : UIPopup<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                if (_instance == null)
                    Debug.LogError($"[{typeof(T).Name}] not found in the scene.");
            }

            return _instance;
        }
    }

    [SerializeField] private Button closeButton;
    private PopupAnimator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<PopupAnimator>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        else
            Debug.LogWarning($"{name} has no Close button assigned.");

        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize()
    {

    }

    public virtual void Show()
    {
        if (animator == null)
        {
            animator = GetComponent<PopupAnimator>();
        }

        gameObject.SetActive(true);
        animator?.Show();
        UIStackManager.Instance?.Push(this);
        OnShow();
    }

    public override void Hide()
    {
        animator?.Hide();
        UIStackManager.Instance?.Pop(this);
        OnHide();
    }

    public virtual void OnBack()
    {
        Hide(); // Default back action
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
