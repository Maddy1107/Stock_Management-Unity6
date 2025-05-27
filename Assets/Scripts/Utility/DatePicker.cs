using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DatePicker : MonoBehaviour
{
    public static DatePicker Instance;
    private Action<int, int, int> onDateSelectedCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ShowDatePicker(Action<int, int, int> onDateSelected = null)
    {
        onDateSelectedCallback = onDateSelected;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = GetActivity())
        using (AndroidJavaObject datePickerDialog = new AndroidJavaObject(
            "android.app.DatePickerDialog",
            activity,
            new DateListener(UpdateDateFromPicker),
            DateTime.Now.Year,
            DateTime.Now.Month,
            DateTime.Now.Day
        ))
        {
            datePickerDialog.Call("show");
        }
#else
        // Simulate date selection in editor
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day;
        Debug.Log($"[Editor Simulation] Picked date: {day:D2}/{month:D2}/{year}");
        UpdateDateFromPicker(day, month, year);
#endif
    }

    private void UpdateDateFromPicker(int day, int month, int year)
    {
        string dateStr = $"{day:D2}/{month:D2}/{year}";
        Debug.Log($"Date selected: {dateStr}");

        onDateSelectedCallback?.Invoke(day, month, year);
    }

    private AndroidJavaObject GetActivity()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    private class DateListener : AndroidJavaProxy
    {
        private readonly Action<int, int, int> callback;

        public DateListener(Action<int, int, int> callback)
            : base("android.app.DatePickerDialog$OnDateSetListener")
        {
            this.callback = callback;
        }

        public void onDateSet(AndroidJavaObject view, int year, int month, int day)
        {
            callback?.Invoke(day, month + 1, year); // +1 to convert 0-based Android month
        }
    }
}
