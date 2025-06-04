using TMPro;
using UnityEngine;

public class FinalListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_InputField productValueText;

    public void SetData(string productName, string productValue)
    {
        if (productNameText != null)
        {
            productNameText.text = productName;
        }

        if (productValueText != null)
        {
            productValueText.text = productValue;
        }
    }
}
