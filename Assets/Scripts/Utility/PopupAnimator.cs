using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class PopupAnimator : MonoBehaviour
{
    private RectTransform popupContent;
    private CanvasGroup canvasGroup;

    public float animationDuration = 0.25f;

    private Coroutine animCoroutine;

    void Awake()
    {
        if (popupContent == null)
            popupContent = transform.GetChild(0).GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        popupContent.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(bool isPopup = true)
    {
        gameObject.SetActive(true);

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(AnimatePopup(Vector3.one, 1f, true, null, isPopup));
    }

    public void Hide(bool isPopup = true,Action onComplete = null)
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(AnimatePopup(Vector3.zero, 0f, false, onComplete, isPopup));
    }

    private IEnumerator AnimatePopup(Vector3 targetScale, float targetAlpha, bool enableOnFinish, Action onComplete, bool animateScale)
    {
        float t = 0f;
        Vector3 startScale = popupContent.localScale;
        float startAlpha = canvasGroup.alpha;

        canvasGroup.blocksRaycasts = true;

        if (!animateScale)
            popupContent.localScale = Vector3.one;

        while (t < animationDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / animationDuration);

            if (animateScale)
                popupContent.localScale = Vector3.Lerp(startScale, targetScale, progress);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            yield return null;
        }

        if (animateScale)
            popupContent.localScale = targetScale;
        else
            popupContent.localScale = Vector3.one;


        canvasGroup.alpha = targetAlpha;

        canvasGroup.interactable = enableOnFinish;
        canvasGroup.blocksRaycasts = enableOnFinish;

        onComplete?.Invoke();
    }

}
