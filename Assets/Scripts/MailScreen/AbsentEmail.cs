using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbsentEmail : MonoBehaviour
{
    [SerializeField] private TMP_InputField numberOfDatesInput;
    [SerializeField] private GameObject selectDatePrefab;
    [SerializeField] private Transform dateContainer;
    [SerializeField] private TMP_Text absentText;
    [SerializeField] private Button copyButton;

    private TMP_Text[] selectedDates;
    private string[] _previousDates;

    private void OnEnable()
    {
        ResetUI();
        numberOfDatesInput.onValueChanged.AddListener(OnNumberOfDatesChanged);
        numberOfDatesInput.ActivateInputField();
        copyButton.onClick.AddListener(() => MailScreen.Instance.CopyToClipboard());
    }

    private void OnDisable()
    {
        numberOfDatesInput.onValueChanged.RemoveListener(OnNumberOfDatesChanged);
        copyButton.onClick.RemoveListener(() => MailScreen.Instance.CopyToClipboard());
    }

    private void OnNumberOfDatesChanged(string input)
    {
        ClearChildren(dateContainer);

        if (!int.TryParse(input, out int count) || count <= 0)
        {
            Debug.LogWarning("Invalid number of dates entered.");
            return;
        }

        selectedDates = new TMP_Text[count];

        for (int i = 0; i < count; i++)
        {
            GameObject dateGO = Instantiate(selectDatePrefab, dateContainer);
            var buttonText = dateGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Select Date";

            selectedDates[i] = dateGO.GetComponentInChildren<TMP_Text>();
            dateGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                DatePicker.Instance.Show(dateGO.GetComponent<Button>());
            });
        }
    }

    void Update()
    {
        if (selectedDates != null)
        {
            bool changed = false;
            if (_previousDates == null || _previousDates.Length != selectedDates.Length)
            {
                _previousDates = new string[selectedDates.Length];
                changed = true;
            }

            HashSet<string> seenDates = new HashSet<string>();
            bool duplicateFound = false;

            for (int i = 0; i < selectedDates.Length; i++)
            {
                string currentText = selectedDates[i]?.text ?? "";
                if (_previousDates[i] != currentText)
                {
                    _previousDates[i] = currentText;
                    changed = true;
                }

                if (!string.IsNullOrEmpty(currentText) && currentText != "Select Date")
                {
                    if (!seenDates.Add(currentText))
                    {
                        duplicateFound = true;
                        selectedDates[i].text = "Select Date";
                        GUIManager.Instance.ShowAndroidToast("Duplicate date selected. Please select unique dates.");
                    }
                }
            }

            if (changed && !duplicateFound)
            {
                BuildEmail();
            }
            
        }

        if (selectedDates != null && selectedDates.Length > 0)
        {
            bool allSelected = true;
            HashSet<string> uniqueDates = new HashSet<string>();

            foreach (var date in selectedDates)
            {
                string text = date?.text ?? "";
                if (string.IsNullOrEmpty(text) || text == "Select Date" || !uniqueDates.Add(text))
                {
                    allSelected = false;
                    break;
                }
            }

            copyButton.interactable = allSelected;
        }
        else
        {
            copyButton.interactable = false;
        }
    }

    private void BuildEmail()
    {
        var sb = new StringBuilder("This is to inform that the following dates are marked as absent in GreytHR:\n\n");

        bool hasDates = selectedDates != null && selectedDates.Length > 0;
        int count = 0;

        if (hasDates)
        {
            foreach (var date in selectedDates)
            {
                if (!string.IsNullOrEmpty(date.text) && date.text != "Select Date")
                {
                    sb.Append($"{date.text}, ");
                    hasDates = true;
                }
                else
                {
                    count++;
                }
            }

            sb.Length -= 2; // Trim last comma
            sb.AppendLine();
        }
        if(!hasDates || count == selectedDates.Length)
        {
            sb.AppendLine();
            sb.AppendLine("No Dates Selected.");
        }

        sb.AppendLine("\nIt will be very nice if you can fix the issue.");

        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString(), absentText);
    }

    public void ResetUI()
    {
        numberOfDatesInput.text = "";
        selectedDates = null;
        ClearChildren(dateContainer);
        BuildEmail();
    }

    private void ClearChildren(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
