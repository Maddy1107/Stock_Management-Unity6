using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupAnimator))]
public abstract class Popup<T> : PopupBase where T : Popup<T>
{
    public static T Instance { get; private set; }

    [SerializeField] private Button closeButton;
    protected PopupAnimator animator;

    protected virtual void Awake()
    {
        if (Instance == null)
            Instance = this as T;
        else
        {
            Destroy(gameObject);
            return;
        }

        animator = GetComponent<PopupAnimator>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        animator?.Show();
        PopupStackManager.Instance?.Register(this);
        OnShow();
    }

    public override void Hide()
    {
        animator?.Hide();
        gameObject.SetActive(false);
        PopupStackManager.Instance?.Deregister(this);
        OnHide();
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
