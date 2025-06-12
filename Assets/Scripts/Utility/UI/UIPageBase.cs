using UnityEngine;

public abstract class UIPageBase : MonoBehaviour
{
    public static UIPageBase CurrentVisiblePage { get; private set; }

    public static void SetCurrentPage(UIPageBase page)
    {
        if (CurrentVisiblePage != null && CurrentVisiblePage != page)
            CurrentVisiblePage.Hide();

        CurrentVisiblePage = page;
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);

        if (CurrentVisiblePage == this)
            CurrentVisiblePage = null;
    }
}
