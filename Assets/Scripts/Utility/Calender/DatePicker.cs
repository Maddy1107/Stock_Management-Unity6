using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupAnimator))]
public class DatePicker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform dayGrid;
    [SerializeField] private Button dayButtonPrefab;
    [SerializeField] private Button prevMonthButton;
    [SerializeField] private Button nextMonthButton;
    [SerializeField] private TMP_Text monthYearText;

    [Header("Button Sprites")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite todaySprite;

    private Button _targetButton;
    private DateTime _currentMonth;
    private readonly List<Button> _dayButtons = new();

    public static DatePicker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        prevMonthButton.onClick.AddListener(() =>
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            BuildCalendar();
        });

        nextMonthButton.onClick.AddListener(() =>
        {
            _currentMonth = _currentMonth.AddMonths(1);
            BuildCalendar();
        });
    }

    public void Show(Button targetButton)
    {
        _targetButton = targetButton;
        GetComponent<PopupAnimator>()?.Show();
        BuildCalendar();
    }

    private void BuildCalendar()
    {
        ClearDayButtons();
        monthYearText.text = _currentMonth.ToString("MMMM yyyy");

        DateTime prevMonth = _currentMonth.AddMonths(-1);
        DateTime nextMonth = _currentMonth.AddMonths(1);
        DateTime thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
        int startDay = (int)_currentMonth.DayOfWeek;

        nextMonthButton.interactable = _currentMonth < thisMonth;
        prevMonthButton.interactable = _currentMonth > new DateTime(2020, 1, 1);

        // Leading days
        int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        for (int i = startDay - 1; i >= 0; i--)
            CreateDayButton(new DateTime(prevMonth.Year, prevMonth.Month, daysInPrevMonth - i), true);

        // Current month days
        for (int day = 1; day <= daysInMonth; day++)
            CreateDayButton(new DateTime(_currentMonth.Year, _currentMonth.Month, day), false);

        // Trailing days
        int totalCells = startDay + daysInMonth;
        int trailing = (7 - (totalCells % 7)) % 7;
        for (int i = 1; i <= trailing; i++)
            CreateDayButton(new DateTime(nextMonth.Year, nextMonth.Month, i), true);
    }

    private void CreateDayButton(DateTime date, bool isFromOtherMonth)
    {
        Button button = Instantiate(dayButtonPrefab, dayGrid);
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        Image bgImage = button.GetComponent<Image>();

        label.text = date.Day.ToString();

        bool isFutureDate = date.Date > DateTime.Today;
        bool isSelectable = !isFromOtherMonth && !isFutureDate;

        // Set text color
        if (isFromOtherMonth)
            label.color = new Color(0.5f, 0.5f, 0.5f); // dimmed for other months
        else if (isFutureDate)
            label.color = new Color(0.7f, 0.7f, 0.7f); // light gray for future dates
        else
            label.color = Color.black;

        // Sprite for today
        bgImage.sprite = date.Date == DateTime.Today ? todaySprite : defaultSprite;

        button.interactable = isSelectable;

        if (isSelectable)
        {
            button.onClick.AddListener(() =>
            {
                _targetButton.GetComponentInChildren<TMP_Text>().text = date.ToString("dd MMM");
                GetComponent<PopupAnimator>()?.Hide();
            });
        }

        _dayButtons.Add(button);
    }


    private void ClearDayButtons()
    {
        foreach (var button in _dayButtons)
        {
            Destroy(button.gameObject);
        }
        _dayButtons.Clear();
    }
}
