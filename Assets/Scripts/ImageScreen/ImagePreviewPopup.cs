using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ImagePreviewPopup : UIPopup<ImagePreviewPopup>
{
    [SerializeField] private Image previewImage;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform imageContainer;

    private Vector2 lastPanPosition;
    private bool isPanning;
    private float doubleTapTimer;
    private const float doubleTapThreshold = 0.3f;
    private Vector2 velocity;
    private float inertiaDecay = 5f;

    private void OnDisable()
    {
        ResetZoom();
    }

    public void ShowImage(Sprite sprite)
    {
        if (sprite != null)
        {
            previewImage.sprite = sprite;
            ResetZoom();
            Show();
        }
    }

    private void ResetZoom()
    {
        imageContainer.localScale = Vector3.one;
        imageContainer.anchoredPosition = Vector2.zero;
        velocity = Vector2.zero;
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        HandleTouchZoom();
        HandleTouchPan();
        HandleDoubleTap();
#elif UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseZoom();
        HandleMousePan();
        HandleMouseDoubleClick();
#endif
        ApplyInertia();
        ClampImagePosition();
    }

    // ----------- Zoom Handling -----------

    private void Zoom(float increment)
    {
        Vector3 newScale = imageContainer.localScale + Vector3.one * increment;
        newScale = Vector3.Max(Vector3.one * 0.5f, Vector3.Min(Vector3.one * 3f, newScale));
        imageContainer.localScale = newScale;
        ClampImagePosition();
    }

    private void HandleMouseZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y * 0.1f;
        if (Mathf.Abs(scrollDelta) > 0.01f)
            Zoom(scrollDelta);
    }

    private void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDist = Vector2.Distance(prev0, prev1);
            float currDist = Vector2.Distance(t0.position, t1.position);

            float delta = (currDist - prevDist) * 0.01f;
            Zoom(delta);
        }
    }

    // ----------- Pan Handling -----------

    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isPanning)
        {
            Vector2 current = Input.mousePosition;
            Vector2 delta = current - lastPanPosition;
            imageContainer.anchoredPosition += delta;
            velocity = delta / Time.deltaTime;
            lastPanPosition = current;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }
    }

    private void HandleTouchPan()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isPanning = true;
                lastPanPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Moved && isPanning)
            {
                Vector2 delta = touch.position - lastPanPosition;
                imageContainer.anchoredPosition += delta;
                velocity = delta / Time.deltaTime;
                lastPanPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isPanning = false;
            }
        }
    }

    private void ApplyInertia()
    {
        if (!isPanning && velocity.magnitude > 1f)
        {
            imageContainer.anchoredPosition += velocity * Time.deltaTime;
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * inertiaDecay);
        }
    }

    private void ClampImagePosition()
    {
        if (previewImage.sprite == null) return;

        Vector2 canvasSize = ((RectTransform)scrollRect.viewport).rect.size;
        Vector2 scaledSize = previewImage.sprite.bounds.size * imageContainer.localScale.x * previewImage.pixelsPerUnit;

        Vector2 clamped = imageContainer.anchoredPosition;

        float xMax = Mathf.Max(0, (scaledSize.x - canvasSize.x) / 2);
        float yMax = Mathf.Max(0, (scaledSize.y - canvasSize.y) / 2);

        clamped.x = Mathf.Clamp(clamped.x, -xMax, xMax);
        clamped.y = Mathf.Clamp(clamped.y, -yMax, yMax);

        imageContainer.anchoredPosition = clamped;
    }

    // ----------- Double-Tap to Reset -----------

    private void HandleDoubleTap()
    {
        if (Input.touchCount == 1)
        {
            Touch tap = Input.GetTouch(0);
            if (tap.tapCount == 2)
            {
                ResetZoom();
            }
        }
    }

    private void HandleMouseDoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - doubleTapTimer < doubleTapThreshold)
            {
                ResetZoom();
                doubleTapTimer = 0;
            }
            else
            {
                doubleTapTimer = Time.time;
            }
        }
    }

    public void Close()
    {
        Hide();
    }
}
