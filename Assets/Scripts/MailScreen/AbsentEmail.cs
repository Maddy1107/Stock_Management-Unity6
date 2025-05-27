using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbsentEmail : MonoBehaviour
{
    public TMP_InputField numberOfDates;

    public GameObject selectDataPrefab;
    public GameObject dateContainer;
    public TMP_Text absentText;

    void OnEnable()
    {
        BuildAbsentEmail();
        numberOfDates.onValueChanged.RemoveAllListeners();
        numberOfDates.onValueChanged.AddListener(OnNumberOfDatesChanged);
    }

    public void OnNumberOfDatesChanged(string value)
    {
        foreach (Transform child in dateContainer.transform)
        {
            Destroy(child.gameObject);
        }

        int count;
        if (int.TryParse(value, out count) && count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject dateObject = Instantiate(selectDataPrefab, dateContainer.transform);
                dateObject.GetComponentInChildren<TMP_Text>().text = $"Select Date {i + 1}";
                dateObject.GetComponent<Button>().onClick.RemoveAllListeners();
                dateObject.GetComponent<Button>().onClick.AddListener(() => SelectDate(dateObject));
            }
        }
        else
        {
            Debug.LogWarning("Invalid number of dates entered.");
        }
    }

    public List<string> dates = new List<string>();

    public void SelectDate(GameObject dateObject)
    {
        DatePicker.Instance.ShowDatePicker((day, month, year) =>
        {
            string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            dates.Add($"{day:D2} {monthName}");
            dateObject.GetComponentInChildren<TMP_Text>().text = $"{day:D2} {monthName}";
            BuildAbsentEmail();
        });

    }

    public void BuildAbsentEmail()
    {
        var sb = new StringBuilder();

        sb.AppendLine("This is to inform that the following dates are marked as absent in GreytHR:");
        sb.AppendLine();

        if (dates.Count == 0)
        {
            sb.AppendLine("No Dates Selected.");
        }
        foreach (var date in dates)
        {
            sb.Append($"{date},");
        }

        sb.AppendLine();
        sb.AppendLine("It will be very nice if you can fix the issue.");

        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString().TrimEnd(','),absentText);
    }

}
