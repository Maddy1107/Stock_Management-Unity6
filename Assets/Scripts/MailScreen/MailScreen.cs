using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MailType
{
    Required,
    Received,
    Absent
}

public class MailScreen : UIPage<MailScreen>
{
    [Header("Page References")]
    [SerializeField] private ProductMail productMailPage;
    [SerializeField] private AbsentEmail absentMailPage;

    #region Unity Events

    protected override void Awake()
    {
        base.Awake();
        GameEvents.OnTypeSelected += HandleMailTypeSelected;
    }

    private void OnDestroy()
    {
        GameEvents.OnTypeSelected -= HandleMailTypeSelected;
    }

    protected override void OnShow()
    {
        ResetMailPages();
    }

    #endregion

    #region Mail Type Handler

    public void HandleMailTypeSelected(MailType type)
    {
        ResetMailPages();

        if (type == MailType.Absent)
            absentMailPage.Show();
        else
            productMailPage.Show(type);
    }

    private void ResetMailPages()
    {
        productMailPage?.Hide();
        absentMailPage?.Hide();
    }

    #endregion

    
}
