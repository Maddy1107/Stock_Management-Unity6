using System.Collections.Generic;
using UnityEngine;

public class UIStackManager : MonoBehaviour
{
    private static UIStackManager _instance;
    public static UIStackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<UIStackManager>();
                if (_instance == null)
                {
                    Debug.LogError("No UIStackManager found in scene.");
                }
            }
            return _instance;
        }
    }

    private readonly Stack<IUIStackElement> stack = new Stack<IUIStackElement>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBack();
        }
    }

    public void Push(IUIStackElement ui)
    {
        if (ui != null && !stack.Contains(ui))
        {
            stack.Push(ui);
        }
    }

    public void Pop(IUIStackElement ui)
    {
        if (stack.Count == 0) return;

        if (stack.Peek() == ui)
        {
            stack.Pop();
        }
        else
        {
            // Cleanup in case something is hiding out of order
            var temp = new Stack<IUIStackElement>();
            while (stack.Count > 0)
            {
                var top = stack.Pop();
                if (top != ui)
                    temp.Push(top);
                else
                    break;
            }
            while (temp.Count > 0)
                stack.Push(temp.Pop());
        }
    }

    private void HandleBack()
    {
        if (stack.Count > 0)
        {
            IUIStackElement top = stack.Peek();
            top.OnBack();
        }
        else
        {
            Debug.Log("UI stack empty. Quitting app.");
            Application.Quit();
        }
    }
}
