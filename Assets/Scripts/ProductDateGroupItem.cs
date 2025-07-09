using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductDateGroupItem : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text receivedText;
    [SerializeField] private TMP_Text requestedText;
    [SerializeField] private Button collapseButton;
    [SerializeField] private Collapsible collapsibleSection;

    private HashSet<int> groupProductIds = new(); // Track only IDs
    private int receivedCount = 0;

    private void OnEnable()
    {
        collapseButton.onClick.AddListener(() => collapsibleSection.ToggleSection());
        GameEvents.OnMarkedRecieved += OnProductMarkedReceived;
    }

    private void OnDisable()
    {
        collapseButton.onClick.RemoveAllListeners();
        GameEvents.OnMarkedRecieved -= OnProductMarkedReceived;
    }

    public void Setup(DBAPI.RequestGroup group)
    {
        // Set collapsible content
        collapsibleSection.SetProducts(group.requests);

        // Set date
        dateText.text = DateTime.TryParse(group.date, out var parsedDate)
            ? parsedDate.ToString("dd MMMM")
            : group.date;

        // Reset
        groupProductIds.Clear();
        receivedCount = 0;

        // Count requests
        requestedText.text = $"{group.requests.Count} REQUESTED";

        foreach (var req in group.requests)
        {
            groupProductIds.Add(req.id);
            if (req.received)
                receivedCount++;
        }

        UpdateReceivedLabel();
    }

    private void OnProductMarkedReceived(DBAPI.ProductRequest req)
    {
        if (!groupProductIds.Contains(req.id))
            return;

        if (req.received)
        {
            receivedCount++;
        }
        else
        {
            receivedCount = Mathf.Max(0, receivedCount - 1);
        }

        UpdateReceivedLabel();
    }


    private void UpdateReceivedLabel()
    {
        receivedText.text = $"{receivedCount} RECEIVED";
    }
}
