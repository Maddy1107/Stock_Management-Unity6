using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupAnimator))]
public class MonthYearPicker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform monthGrid;
    [SerializeField] private Button monthButtonPrefab;
    [SerializeField] private Transform yearGrid;
    [SerializeField] private Button yearButtonPrefab;

    private Button _targetButton;
    private readonly List<Button> _monthButtons = new();
    private readonly List<Button> _yearButtons = new();

    private int _selectedYear;
    private Action<string, string> _onMonthYearSelected;

    // Use 3-letter month abbreviations in all caps
    private readonly string[] _months = new[]
    {
        "JAN", "FEB", "MAR", "APR", "MAY", "JUN",
        "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
    };

    private readonly int[] _yearOptions = new[] { 2020, 2021, 2022, 2023, 2024, 2025 };

    public static MonthYearPicker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(Button targetButton, Action<string, string> onMonthYearSelected)
    {
        _targetButton = targetButton;
        _onMonthYearSelected = onMonthYearSelected;
        _selectedYear = DateTime.Now.Year;

        GetComponent<PopupAnimator>()?.Show();
        BuildYearButtons();
        BuildMonthGrid();
    }


    private void BuildYearButtons()
    {
        ClearYearButtons();
        for (int i = _yearOptions.Length - 1; i >= 0; i--)
        {
            int capturedYear = _yearOptions[i];
            Button btn = Instantiate(yearButtonPrefab, yearGrid);
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            label.text = capturedYear.ToString();

            if (capturedYear == _selectedYear)
                label.color = Color.red;

            btn.onClick.AddListener(() =>
            {
                _selectedYear = capturedYear;
                BuildYearButtons(); // refresh color highlight
            });

            _yearButtons.Add(btn);
        }
    }

    private void BuildMonthGrid()
    {
        ClearMonthButtons();

        for (int i = 0; i < 12; i++)
        {
            int capturedIndex = i;
            Button btn = Instantiate(monthButtonPrefab, monthGrid);
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            label.text = _months[i];
            label.fontSize = 57;

            btn.onClick.AddListener(() =>
            {
                string month = _months[capturedIndex];
                _targetButton.GetComponentInChildren<TMP_Text>().text = $"{month} {_selectedYear}";
                _onMonthYearSelected?.Invoke(month, _selectedYear.ToString()); // ← callback here
                GetComponent<PopupAnimator>()?.Hide();
            });


            _monthButtons.Add(btn);
        }
    }

    private void ClearMonthButtons()
    {
        foreach (var btn in _monthButtons)
            Destroy(btn.gameObject);
        _monthButtons.Clear();
    }

    private void ClearYearButtons()
    {
        foreach (var btn in _yearButtons)
            Destroy(btn.gameObject);
        _yearButtons.Clear();
    }
}
