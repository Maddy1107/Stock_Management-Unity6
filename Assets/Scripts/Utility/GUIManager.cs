using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        MainMenuPanel.Instance?.Show();
    }

    public void CopyToClipboard(string stringToCopy)
    {
        GUIUtility.systemCopyBuffer = stringToCopy.Trim();
        ShowAndroidToast("Copied to clipboard");
    }
    
    public void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass toastClass = new("android.widget.Toast");
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                toast.Call("show");
            }));
        }
#else
        Debug.Log("Toast: " + message);
#endif
    }
}
