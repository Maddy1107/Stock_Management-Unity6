using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private List<GameObject> panelList;

    [Header("Popups")]
    [SerializeField] private List<GameObject> popupList;

    private Dictionary<string, GameObject> panels = new();
    private Dictionary<string, GameObject> popups = new();
    private Stack<GameObject> popupStack = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterPanels();
        RegisterPopups();
    }

    private void RegisterPanels()
    {
        foreach (var panel in panelList)
        {
            if (panel != null)
                panels[panel.name] = panel;
        }
    }

    private void RegisterPopups()
    {
        foreach (var popup in popupList)
        {
            if (popup != null)
                popups[popup.name] = popup;
        }
    }

    // --- PANEL METHODS ---

    public void ShowPanel(string panelName)
    {
        if (panels.TryGetValue(panelName, out var panel))
            panel.SetActive(true);
    }

    public void HidePanel(string panelName)
    {
        if (panels.TryGetValue(panelName, out var panel))
            panel.SetActive(false);
    }

    public void HideAllPanels()
    {
        foreach (var panel in panels.Values)
            panel.SetActive(false);
    }

    // --- POPUP METHODS ---

    public void ShowPopup(string popupName, bool stack = true)
    {
        if (popups.TryGetValue(popupName, out var popup))
        {
            popup.SetActive(true);
            if (stack)
                popupStack.Push(popup);
        }
    }

    public void HidePopup(string popupName)
    {
        if (popups.TryGetValue(popupName, out var popup))
        {
            popup.SetActive(false);
            if (popupStack.Count > 0 && popupStack.Peek() == popup)
                popupStack.Pop();
        }
    }

    public void HideTopPopup()
    {
        if (popupStack.Count > 0)
        {
            var popup = popupStack.Pop();
            popup.SetActive(false);
        }
    }

    public void HideAllPopups()
    {
        foreach (var popup in popups.Values)
            popup.SetActive(false);
        popupStack.Clear();
    }
}
