using TMPro;
using UnityEngine;

/// <summary>Displays the current game day and follows normal progression and rollback.</summary>
public class DayHUD : MonoBehaviour
{
    private DayManager dayManager;
    private TMP_Text dayText;

    public void Initialize(DayManager manager, TMP_Text label)
    {
        if (dayManager != null) dayManager.OnDayStarted -= Refresh;
        dayManager = manager;
        dayText = label;
        if (isActiveAndEnabled && dayManager != null) dayManager.OnDayStarted += Refresh;
        Refresh(dayManager == null ? 1 : dayManager.CurrentDay);
    }

    private void OnEnable()
    {
        if (dayManager != null) dayManager.OnDayStarted += Refresh;
    }

    private void OnDisable()
    {
        if (dayManager != null) dayManager.OnDayStarted -= Refresh;
    }

    private void Refresh(int day)
    {
        if (dayText != null) dayText.text = "DAY - " + Mathf.Max(1, day);
    }
}
