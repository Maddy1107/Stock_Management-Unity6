using UnityEngine;
using UnityEngine.UI;

public enum CalenderType
{
    None,
    DatePicker,
    MonthYearPicker
}

public class Calender : MonoBehaviour
{
    public static Calender Instance { get; private set; }

    public GameObject datePickerPrefab;
    public GameObject monthYearPickerPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Show(CalenderType CurrentCalenderType, Button targetButton, System.Action<string, string> onMonthYearSelected = null)
    {
        if (CurrentCalenderType == CalenderType.None)
        {
            Debug.LogWarning("No calendar type selected. Please use ShowDatePicker or ShowMonthYearPicker.");
            return;
        }

        if (CurrentCalenderType == CalenderType.DatePicker)
        {
            ShowDatePicker(targetButton);
        }
        else if (CurrentCalenderType == CalenderType.MonthYearPicker)
        {
            ShowMonthYearPicker(targetButton, onMonthYearSelected);
        }
    }

    public static void ShowDatePicker(Button targetButton)
    {
        DatePicker.Instance.Show(targetButton);
    }

    public void ShowMonthYearPicker(Button targetButton, System.Action<string, string> onMonthYearSelected)
    {
        MonthYearPicker.Instance.Show(targetButton, onMonthYearSelected);
    }
}
