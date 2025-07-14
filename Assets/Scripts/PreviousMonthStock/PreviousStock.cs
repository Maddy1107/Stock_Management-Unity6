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
        openPickerButton.onClick.AddListener(OnPickerButtonClicked);
    }

    private void OnDisable()
    {
        openPickerButton.onClick.RemoveListener(OnPickerButtonClicked);
    }

    public override void Show()
    {
        base.Show();
        ResetPopup();
    }
    private void OnPickerButtonClicked()
    {
        Calender.Instance.Show(CalenderType.MonthYearPicker, openPickerButton, OnMonthYearPicked);
    }

    private void OnMonthYearPicked(string month, string year)
    {
        openPickerButton.GetComponentInChildren<TMP_Text>().text = $"{month} {year}";
        FetchProductData(month, year);
    }

    private void FetchProductData(string month, string year)
    {
        LoadingScreen.Instance.Show();

        DBAPI.Instance.FetchProductData(
            month, year,
            onSuccess: data =>
            {
                ClearItems();

                if (data == null || data.Count == 0)
                {
                    noDataText.gameObject.SetActive(true);
                }
                else
                {
                    noDataText.gameObject.SetActive(false);
                    foreach (var pair in data)
                    {
                        var itemGO = Instantiate(itemPrefab, itemContainer);
                        if (itemGO.TryGetComponent(out FinalListItem item))
                        {
                            item.SetData(pair.Key, pair.Value[0], pair.Value[1]);
                        }
                    }
                }

                LoadingScreen.Instance.Hide();
            },
            onError: error =>
            {
                Debug.LogError($"Failed to fetch product data: {error}");
                GUIManager.Instance.ShowAndroidToast("Unable to load data. Please try again.");
                LoadingScreen.Instance.Hide();
            }
        );
    }

    private void ClearItems()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void ResetPopup()
    {
        openPickerButton.GetComponentInChildren<TMP_Text>().text = "Select Month & Year";
        ClearItems();
        noDataText.gameObject.SetActive(true);
    }
}
