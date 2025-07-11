using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressSlider;
    public Image fadeOverlay;
    public TMP_Text statusText;
    public Transform fillHandle;

    private BackendLoader backendLoader;

    private float visualProgress = 0f;
    private float targetProgress = 0f;
    private bool backendReady = false;
    private string nextScene = "MainScene";

    void Start()
    {
        backendLoader = new BackendLoader();
        StartCoroutine(RunStartup());
        StartCoroutine(SmoothProgressRoutine());
        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RunStartup()
    {
        backendLoader = new BackendLoader();
        backendLoader.OnStatusUpdate += UpdateStatus;
        backendLoader.OnProgressUpdate += (p) => targetProgress = p;

        yield return backendLoader.StartBackendSetup();

        backendReady = true;
        UpdateStatus("Finalizing...");

        // 👇 Wait until the slider finishes visually reaching 1.0
        while (visualProgress < 0.995f)
            yield return null;

        yield return new WaitForSeconds(0.3f); // slight buffer for feel

        yield return FadeIn();

        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator SmoothProgressRoutine()
    {
        while (!backendReady || visualProgress < 1f)
        {
            visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, Time.deltaTime * 0.25f);
            if (progressSlider != null)
                progressSlider.value = visualProgress;

            yield return null;
        }
    }

    private IEnumerator RotateRoutine()
    {
        while (true && progressSlider.value != 1)
        {
            Quaternion startRotation = fillHandle.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 180);
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                fillHandle.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null)
            yield break;

        float t = 0f;
        float duration = 1f;
        Color start = fadeOverlay.color;
        Color end = new Color(start.r, start.g, start.b, 1f);

        while (t < duration)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0, 1, t / duration); // 👈 smooth easing
            fadeOverlay.color = Color.Lerp(start, end, eased);
            yield return null;
        }
        fadeOverlay.color = end;
    }


    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
