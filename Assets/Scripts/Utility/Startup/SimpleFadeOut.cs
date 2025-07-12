using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleFadeOut : MonoBehaviour
{
    public Image fadeOverlay;         // Assign in Inspector
    public float fadeDuration = 1f; // How long the fade takes

    private void Start()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.enabled = true;
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        if (fadeOverlay == null)
            yield break;

        fadeOverlay.raycastTarget = false;

        float t = 0f;
        float duration = 2f;
        Color start = fadeOverlay.color;
        Color end = new Color(start.r, start.g, start.b, 0f);

        while (t < duration)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0, 1, t / duration); // 👈 smooth easing
            fadeOverlay.color = Color.Lerp(start, end, eased);
            yield return null;
        }
        fadeOverlay.color = end;
    }
}
