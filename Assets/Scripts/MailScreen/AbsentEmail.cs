using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbsentEmail : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField numberOfDatesInput;
    [SerializeField] private GameObject selectDatePrefab;
    [SerializeField] private Transform dateContainer;
    [SerializeField] private TMP_Text absentText;
    [SerializeField] private Button copyButton;

    private TMP_Text[] selectedDates;
    private string[] previousDates;

    private void OnEnable()
    {
        ResetUI();
        numberOfDatesInput.onValueChanged.AddListener(HandleDateCountChanged);
        copyButton.onClick.AddListener(OnCopyClicked);
        numberOfDatesInput.ActivateInputField();

        BuildEmail();
    }

    private void OnDisable()
    {
        numberOfDatesInput.onValueChanged.RemoveListener(HandleDateCountChanged);
        copyButton.onClick.RemoveAllListeners();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void ResetUI()
    {
        numberOfDatesInput.text = "";
        selectedDates = null;
        previousDates = null;
        ClearChildren(dateContainer);
        absentText.text = "";
    }

    private void HandleDateCountChanged(string input)
    {
        ClearChildren(dateContainer);

        if (!int.TryParse(input, out int count) || count <= 0)
        {
            Debug.LogWarning("Invalid number of dates entered.");
            selectedDates = null;
            return;
        }

        selectedDates = new TMP_Text[count];

        for (int i = 0; i < count; i++)
        {
            GameObject dateGO = Instantiate(selectDatePrefab, dateContainer);
            TMP_Text buttonText = dateGO.GetComponentInChildren<TMP_Text>();
            buttonText.text = "Select Date";
            buttonText.fontSize = 45;

            Button btn = dateGO.GetComponent<Button>();
            btn.onClick.AddListener(() => Calender.Instance.Show(CalenderType.DatePicker, btn, null));

            selectedDates[i] = buttonText;
        }
    }

    private void Update()
    {
        if (selectedDates == null || selectedDates.Length == 0)
        {
            copyButton.interactable = false;
            return;
        }

        bool changed = ValidateDates(out bool hasDuplicates);

        if (changed && !hasDuplicates)
            BuildEmail();

        copyButton.interactable = HasAllUniqueValidDates();
    }

    private bool ValidateDates(out bool duplicateFound)
    {
        duplicateFound = false;
        bool changed = false;

        if (previousDates == null || previousDates.Length != selectedDates.Length)
        {
            previousDates = new string[selectedDates.Length];
            changed = true;
        }

        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < selectedDates.Length; i++)
        {
            string text = selectedDates[i]?.text ?? "";

            if (previousDates[i] != text)
            {
                previousDates[i] = text;
                changed = true;
            }

            if (!string.IsNullOrEmpty(text) && text != "Select Date")
            {
                if (!seen.Add(text))
                {
                    duplicateFound = true;
                    selectedDates[i].text = "Select Date";
                    GUIManager.Instance.ShowAndroidToast("Duplicate date selected. Please select unique dates.");
                }
            }
        }

        return changed;
    }

    private bool HasAllUniqueValidDates()
    {
        HashSet<string> dates = new HashSet<string>();

        foreach (var date in selectedDates)
        {
            string text = date?.text ?? "";
            if (string.IsNullOrEmpty(text) || text == "Select Date" || !dates.Add(text))
                return false;
        }

        return true;
    }

    private void BuildEmail()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Dear Team,\n");
        sb.AppendLine("This is to inform that the following dates are marked as absent in GreytHR:\n");

        int count = 0;
        if (selectedDates != null && selectedDates.Length > 0)
        {
            foreach (var date in selectedDates)
            {
                string text = date?.text ?? "";
                if (!string.IsNullOrEmpty(text) && text != "Select Date")
                {
                    sb.Append(text).Append(", ");
                    count++;
                }
            }
        }

        if (count > 0)
        {
            sb.Length -= 2; // Trim trailing comma
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("No Dates Selected.");
        }

        sb.AppendLine("\nIt will be very nice if you can fix the issue.\n");

        sb.AppendLine($"Thank you \n{PlayerPrefs.GetString("SavedUserName", null)}");

        absentText.text = sb.ToString();
    }

    private void OnCopyClicked()
    {
        string finalText = absentText.text;
        string subjectText = $"Missed Days/Checkout Notification";

        if (!string.IsNullOrWhiteSpace(finalText))
        {
            GUIManager.Instance.CopyToClipboard(finalText);
            GUIManager.Instance.OpenEmail(subjectText, finalText);
        }
    }

    private void ClearChildren(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
