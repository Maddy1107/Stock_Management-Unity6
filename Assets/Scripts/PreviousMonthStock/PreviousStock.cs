using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviousStock : UIPopup<PreviousStock>
{
    [SerializeField] private Button openPickerButton;

    private void OnEnable()
    {
        openPickerButton.onClick.AddListener(OnButtonClick);
    }
    private void OnDisable()
    {
        openPickerButton.onClick.RemoveListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Calender.Instance.Show(CalenderType.MonthYearPicker, openPickerButton, OnMonthYearPicked);
    }

    private void OnMonthYearPicked(string selectedMonthYear)
    {
        openPickerButton.GetComponentInChildren<TMP_Text>().text = selectedMonthYear;
    }
}
