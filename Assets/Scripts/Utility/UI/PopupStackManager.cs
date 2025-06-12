using System.Collections.Generic;
using UnityEngine;

public class PopupStackManager : MonoBehaviour
{
    public static PopupStackManager Instance { get; private set; }

    private readonly Stack<PopupBase> popupStack = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || IsBackButtonPressed())
        {
            if (popupStack.Count > 0)
            {
                popupStack.Peek().Hide();
            }
        }
    }

    private bool IsBackButtonPressed()
    {
#if UNITY_ANDROID
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }

    public void Register(PopupBase popup)
    {
        if (!popupStack.Contains(popup))
            popupStack.Push(popup);
    }

    public void Deregister(PopupBase popup)
    {
        if (popupStack.Count == 0) return;

        // Top only
        if (popupStack.Peek() == popup)
            popupStack.Pop();
    }
}
