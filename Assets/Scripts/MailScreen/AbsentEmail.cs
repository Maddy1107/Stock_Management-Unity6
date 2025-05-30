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
    private string[] _previousDates;

    private void OnEnable()
    {
        ResetUI();
        numberOfDatesInput.onValueChanged.AddListener(HandleDateCountChanged);
        copyButton.onClick.AddListener(OnCopyClicked);
        numberOfDatesInput.ActivateInputField();
    }

    private void OnDisable()
    {
        numberOfDatesInput.onValueChanged.RemoveListener(HandleDateCountChanged);
        copyButton.onClick.RemoveListener(OnCopyClicked);
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
            CreateDateButton(i);
        }
    }

    private void CreateDateButton(int index)
    {
        GameObject dateGO = Instantiate(selectDatePrefab, dateContainer);
        TMP_Text buttonText = dateGO.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
            buttonText.text = "Select Date";

        Button btn = dateGO.GetComponent<Button>();
        btn.onClick.AddListener(() => DatePicker.Instance.Show(btn));

        selectedDates[index] = buttonText;
    }

    private void Update()
    {
        if (selectedDates == null || selectedDates.Length == 0)
        {
            copyButton.interactable = false;
            return;
        }

        bool changed = ValidateSelectedDates(out bool hasDuplicates);

        if (changed && !hasDuplicates)
            BuildEmail();

        copyButton.interactable = HasAllUniqueValidDates();
    }

    private bool ValidateSelectedDates(out bool duplicateFound)
    {
        bool changed = false;
        duplicateFound = false;

        if (_previousDates == null || _previousDates.Length != selectedDates.Length)
        {
            _previousDates = new string[selectedDates.Length];
            changed = true;
        }

        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < selectedDates.Length; i++)
        {
            string text = selectedDates[i]?.text ?? "";

            if (_previousDates[i] != text)
            {
                _previousDates[i] = text;
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
        StringBuilder sb = new StringBuilder("This is to inform that the following dates are marked as absent in GreytHR:\n\n");

        bool hasValidDate = selectedDates != null && selectedDates.Length > 0;
        int count = 0;

        if (hasValidDate)
        {
            foreach (var date in selectedDates)
            {
                string text = date?.text ?? "";
                if (!string.IsNullOrEmpty(text) && text != "Select Date")
                {
                    sb.Append(text).Append(", ");
                    hasValidDate = true;
                }
                else
                {
                    count++;
                }
            }

            sb.Length -= 2; // remove trailing comma
            sb.AppendLine();
        }

        if (!hasValidDate || count == selectedDates.Length)
        {
            sb.AppendLine("\nNo Dates Selected.");
        }
        sb.AppendLine("\nIt will be very nice if you can fix the issue.");
        GUIManager.Instance.ShowFinalEmailScreen(sb.ToString(), absentText);
    }

    private void OnCopyClicked()
    {
        MailScreen.Instance.CopyToClipboard();
    }

    public void ResetUI()
    {
        numberOfDatesInput.text = "";
        selectedDates = null;
        _previousDates = null;
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
