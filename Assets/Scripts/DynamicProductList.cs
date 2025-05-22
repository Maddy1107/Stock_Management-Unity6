using UnityEngine;
using UnityEngine.UI;
using TMPro; // Remove if you're not using TextMeshPro

public class DynamicProductList : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject productPrefab;
    public Transform contentParent;

    private int productCounter = 1;

    void Start()
    {
        AddStaticText("etst test test test");
    }

    public void AddProduct()
    {
        GameObject entry = Instantiate(textPrefab, contentParent);
        TMP_Text textComp = entry.GetComponent<TMP_Text>(); // Use Text if not using TMP
        textComp.text = $"{productCounter} ----";
        productCounter++;
    }

    public void AddStaticText(string message)
    {
        GameObject entry = Instantiate(textPrefab, contentParent);
        TMP_Text textComp = entry.GetComponent<TMP_Text>(); // Use Text if not using TMP
        textComp.text = message;
    }

    public void AddFooter()
    {
        AddStaticText("thank you \nPriyanka");
    }
}
