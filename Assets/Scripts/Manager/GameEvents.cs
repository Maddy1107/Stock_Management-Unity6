using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<ProductItem> OnToggleClicked;
    public static event Action OnUpdateSubmitted;
    public static event Action<MailType> OnTypeSelected;
    public static event Action<FinalListItem> OnEditToggleClicked;
    public static event Action<DBAPI.ProductRequest> OnMarkedRecieved;

    internal static void InvokeOnToggleClicked(ProductItem productItem)
    {
        OnToggleClicked?.Invoke(productItem);
    }
    internal static void InvokeOnUpdateSubmitted()
    {
        OnUpdateSubmitted?.Invoke();
    }
    internal static void InvokeOnTypeSelected(MailType type)
    {
        OnTypeSelected?.Invoke(type);
    }
    internal static void InvokeOnEditToggleClicked(FinalListItem finalListItem)
    {
        OnEditToggleClicked?.Invoke(finalListItem);
    }
    internal static void InvokeOnMarkRecieved(DBAPI.ProductRequest prod)
    {
        OnMarkedRecieved?.Invoke(prod);
    }
}
