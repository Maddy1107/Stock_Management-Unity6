using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class PopupAnimator : MonoBehaviour
{
    [Tooltip("The RectTransform of the popup content that scales in/out")]
    private RectTransform popupContent;

    [Tooltip("CanvasGroup controlling the background fade and interactivity")]
    private CanvasGroup canvasGroup;

    public float animationDuration = 0.25f;

    private Coroutine animCoroutine;

    void Awake()
    {
        // Initially hidden: scale content to zero and fade out canvas group
        if (popupContent == null)
            popupContent = transform.GetChild(0).GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        popupContent.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(AnimatePopup(Vector3.one, 1f, true));
    }

    public void Hide()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(AnimatePopup(Vector3.zero, 0f, false));
    }

    private IEnumerator AnimatePopup(Vector3 targetScale, float targetAlpha, bool enableOnFinish)
    {
        float t = 0f;
        Vector3 startScale = popupContent.localScale;
        float startAlpha = canvasGroup.alpha;

        // Enable raycast blocking so clicks are caught during animation
        canvasGroup.blocksRaycasts = true;

        while (t < animationDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / animationDuration);

            popupContent.localScale = Vector3.Lerp(startScale, targetScale, progress);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            yield return null;
        }

        popupContent.localScale = targetScale;
        canvasGroup.alpha = targetAlpha;

        canvasGroup.interactable = enableOnFinish;
        canvasGroup.blocksRaycasts = enableOnFinish;
    }
}
