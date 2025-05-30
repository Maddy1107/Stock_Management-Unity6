using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DatePicker : MonoBehaviour
{
    [Header("UI References")]
    public Transform dayGrid; // GridLayoutGroup parent
    public Button dayButtonPrefab;
    public Button prevMonthButton;
    public Button nextMonthButton;
    public TMP_Text monthYearText;

    private Button selectDateButton;

    [Header("Button Sprites")]
    public Sprite defaultSprite;
    public Sprite todaySprite;

    private DateTime currentMonth;
    private List<DateTime> selectedDates = new List<DateTime>();
    private List<Button> dayButtons = new List<Button>();

    public static DatePicker Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: Uncomment if you want this to persist across scenes
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        prevMonthButton.onClick.AddListener(() =>
        {
            currentMonth = currentMonth.AddMonths(-1);
            BuildCalendar();
        });

        nextMonthButton.onClick.AddListener(() =>
        {
            currentMonth = currentMonth.AddMonths(1);
            BuildCalendar();
        });
    }

    public void Show(Button datebutton)
    {
        var animator = GetComponent<PopupAnimator>();
        animator.Show();
        selectDateButton = datebutton;
        BuildCalendar();
    }

    void BuildCalendar()
    {
        monthYearText.text = currentMonth.ToString("MMMM yyyy");

        foreach (var btn in dayButtons)
        {
            Destroy(btn.gameObject);
        }
        dayButtons.Clear();

        int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
        int startDayOfWeek = (int)currentMonth.DayOfWeek; // Sunday = 0

        // Get previous and next month references
        DateTime prevMonth = currentMonth.AddMonths(-1);
        int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        DateTime nextMonth = currentMonth.AddMonths(1);

        DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        nextMonthButton.interactable = currentMonth < now;

        // 1. Leading days from previous month
        for (int i = startDayOfWeek - 1; i >= 0; i--)
        {
            DateTime prevMonthDate = new DateTime(prevMonth.Year, prevMonth.Month, daysInPrevMonth - i);
            CreateDayButton(prevMonthDate, true);
        }

        // 2. Days of current month
        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime currentDate = new DateTime(currentMonth.Year, currentMonth.Month, day);
            CreateDayButton(currentDate, false);
        }

        // 3. Trailing days from next month to fill the week
        int totalCells = startDayOfWeek + daysInMonth;
        int trailingDays = (7 - (totalCells % 7)) % 7; // Ensures a complete last week

        for (int i = 1; i <= trailingDays; i++)
        {
            DateTime nextMonthDate = new DateTime(nextMonth.Year, nextMonth.Month, i);
            CreateDayButton(nextMonthDate, true);
        }
    }



    void CreateDayButton(DateTime date, bool isFromOtherMonth)
    {
        Button dayButton = Instantiate(dayButtonPrefab, dayGrid);
        TextMeshProUGUI buttonText = dayButton.GetComponentInChildren<TextMeshProUGUI>();
        Image buttonImage = dayButton.GetComponent<Image>();

        buttonText.text = date.Day.ToString();

        // Set sprite
        if (date.Date == DateTime.Today)
            buttonImage.sprite = todaySprite;
        else
            buttonImage.sprite = defaultSprite;

        // Dim color for other month
        buttonText.color = isFromOtherMonth ? new Color(0.5f, 0.5f, 0.5f) : Color.black;

        // Disable interaction for other month dates (optional)
        dayButton.interactable = !isFromOtherMonth;

        if (!isFromOtherMonth)
        {
            UpdateButtonSelection(dayButton, date);
            dayButton.onClick.AddListener(() => ToggleDateSelection(date));
        }

        dayButtons.Add(dayButton);
    }



    void ToggleDateSelection(DateTime date)
    {
        selectDateButton.GetComponentInChildren<TMP_Text>().text = date.ToString("dd MMM");
        var animator = GetComponent<PopupAnimator>();
        animator.Hide();
    }

    void UpdateButtonSelection(Button btn, DateTime date)
    {
        ColorBlock colors = btn.colors;
        if (selectedDates.Contains(date))
        {
            // Highlight selected color
            colors.normalColor = Color.green;
            colors.selectedColor = Color.green;
        }
        else
        {
            // Default button color
            colors.normalColor = Color.white;
            colors.selectedColor = Color.white;
        }
        btn.colors = colors;
    }

    public List<DateTime> GetSelectedDates()
    {
        return new List<DateTime>(selectedDates);
    }
}
