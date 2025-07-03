using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviousStock : UIPopup<PreviousStock>
{
    [SerializeField] private Button openPickerButton;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private TMP_Text noDataText;

    private void OnEnable()
    {
        openPickerButton.onClick.AddListener(OnButtonClick);
        noDataText.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        openPickerButton.onClick.RemoveListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Calender.Instance.Show(CalenderType.MonthYearPicker, openPickerButton, OnMonthYearPicked);
    }

    private void OnMonthYearPicked(string month, string year)
    {
        LoadingScreen.Instance.Show();
        openPickerButton.GetComponentInChildren<TMP_Text>().text = $"{month} {year}";

        FetchAndDisplayProductData(month, year);

    }

    private void FetchAndDisplayProductData(string month, string year)
    {
        DBAPI.Instance.FetchProductData(
            month, year,
            data =>
            {
                // Clear previous items
                foreach (Transform child in itemContainer)
                {
                    Destroy(child.gameObject);
                }

                if (data == null || data.Count == 0)
                {
                    noDataText.gameObject.SetActive(true);
                }
                else
                {
                    noDataText.gameObject.SetActive(false);
                    foreach (var pair in data)
                    {
                        var listItemGO = Instantiate(itemPrefab, itemContainer);
                        if (listItemGO.TryGetComponent(out FinalListItem item))
                        {
                            item.SetData(pair.Key, pair.Value[0], pair.Value[1]);
                        }
                    }
                }
                LoadingScreen.Instance.Hide();
            },
            error =>
            {
                Debug.LogError($"Failed to fetch product data: {error}");
                LoadingScreen.Instance.Hide();
            }
        );
    }

    public void ResetPopup()
    {
        openPickerButton.GetComponentInChildren<TMP_Text>().text = "Select Month & Year";
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
