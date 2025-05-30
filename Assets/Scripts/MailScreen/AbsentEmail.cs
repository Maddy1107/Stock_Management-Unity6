using System.Collections.Generic;
using System.Text;
using TMPro;
using UI.Dates;
using UnityEngine;
using UnityEngine.UI;

public class AbsentEmail : MonoBehaviour
{
    public TMP_InputField numberOfDates;
    public GameObject selectDataPrefab;
    public GameObject dateContainer;
    public TMP_Text absentText;

    private string[] dates;

    void OnEnable()
    {
        Reset();
        numberOfDates.onValueChanged.RemoveAllListeners();
        numberOfDates.onValueChanged.AddListener(OnNumberOfDatesChanged);
        numberOfDates.ActivateInputField();
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
            Initialize(count);
            for (int i = 0; i < count; i++)
            {
                GameObject dateObject = Instantiate(selectDataPrefab, dateContainer.transform);
                TMP_Text buttonLabel = dateObject.GetComponentInChildren<TMP_Text>();
                if (buttonLabel != null)
                    buttonLabel.text = $"Select Date";
                
                dateObject.GetComponent<DatePicker>().currentIndex = i;
            }
        }
        else
        {
            Debug.LogWarning("Invalid number of dates entered.");
        }
    }

    private void Initialize(int count)
    {
        dates = new string[count];
    }

    public void PickDate(string date, int index)
    {
        if (index >= 0 && index < dates.Length)
        {
            dates[index] = date;

            if (dateContainer.transform.childCount > index)
            {
                var buttonText = dateContainer.transform.GetChild(index).GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = date;
                }
            }

            BuildAbsentEmail();
        }
        else
        {
            Debug.LogWarning($"Invalid index {index} for setting date.");
        }
    }


    private void BuildAbsentEmail()
    {
        var sb = new StringBuilder();
        sb.AppendLine("This is to inform that the following dates are marked as absent in GreytHR:");
        sb.AppendLine();

        bool hasDates = false;
        foreach (var date in dates)
        {
            if (!string.IsNullOrEmpty(date))
            {
                sb.Append($"{date}, ");
                hasDates = true;
            }
        }
        if (hasDates)
        {
            sb.AppendLine();
        }

        if (!hasDates)
        {
            sb.AppendLine("No Dates Selected.");
        }

        sb.AppendLine();
        sb.AppendLine("It will be very nice if you can fix the issue.");

        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString().TrimEnd(',', ' '), absentText);
    }

    public void Reset()
    {
        numberOfDates.text = "";
        dates = new string[0];

        foreach (Transform child in dateContainer.transform)
        {
            Destroy(child.gameObject);
        }

        BuildAbsentEmail();
    }
}
