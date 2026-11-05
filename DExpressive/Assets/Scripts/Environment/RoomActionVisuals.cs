using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Leaves small visible traces in the room after selected daily actions.</summary>
public class RoomActionVisuals : MonoBehaviour
{
    private ChoiceManager choiceManager;
    private DayManager dayManager;
    private TMP_Text notice;

    private void Start()
    {
        choiceManager = FindFirstObjectByType<ChoiceManager>();
        if (choiceManager != null) choiceManager.OnChoiceSelected += ApplyResult;
        dayManager = FindFirstObjectByType<DayManager>();
        if (dayManager != null) dayManager.OnDayStarted += ResetDailyProps;
        CreateNotice();
        ResetDailyProps(dayManager == null ? 1 : dayManager.CurrentDay);
    }

    private void OnDestroy()
    {
        if (choiceManager != null) choiceManager.OnChoiceSelected -= ApplyResult;
        if (dayManager != null) dayManager.OnDayStarted -= ResetDailyProps;
    }

    private void ApplyResult(string choiceId, string optionId, int mentalChange)
    {
        // choiceId is the interacted object node and stays consistent even when
        // a story option later uses a different result/next-story identifier.
        if (WasConsumed(choiceId, optionId, "meal")) SetVisible("Meal - Interact", false);
        if (WasConsumed(choiceId, optionId, "medicine")) SetVisible("Medicine - Interact", false);
        if (WasConsumed(choiceId, optionId, "tidy"))
        {
            SetVisible("Cup - Click to Tidy", false);
            SetVisible("Trash Bin Contents", false);
        }

        if (notice != null)
        {
            string result = mentalChange > 0 ? "+" + mentalChange : mentalChange.ToString();
            notice.text = mentalChange == 0 ? "오늘의 행동이 기록되었습니다" : "정신력 " + result;
            CancelInvoke(nameof(ClearNotice));
            Invoke(nameof(ClearNotice), 2.2f);
        }
    }

    private static void SetVisible(string objectName, bool visible)
    {
        // GameObject.Find ignores disabled objects. That meant a meal or medicine
        // hidden after an interaction could never be found and restored tomorrow.
        GameObject target = FindRoomObject(objectName);
        if (target != null) target.SetActive(visible);
    }

    private static GameObject FindRoomObject(string objectName)
    {
        GameObject activeObject = GameObject.Find(objectName);
        if (activeObject != null) return activeObject;

        // Resources can see runtime scene objects even while they are inactive.
        // Filter to a valid scene so similarly named prefab assets are ignored.
        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate.name == objectName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    private static void ResetDailyProps(int day)
    {
        // Consumables are a new day's routine; rollback also emits OnDayStarted
        // and therefore restores the correct day's visible state.
        SetVisible("Meal - Interact", true);
        SetVisible("Medicine - Interact", true);
        SetVisible("Cup - Click to Tidy", true);
        SetVisible("Trash Bin Contents", true);

        if (GameStateManager.Instance == null) return;
        foreach (ChoiceRecord record in GameStateManager.Instance.CurrentState.choiceHistory)
        {
            if (string.IsNullOrEmpty(record.choiceId) || !record.choiceId.StartsWith("day" + day + "_")) continue;
            if (WasConsumed(record.choiceId, record.selectedOptionId, "meal")) SetVisible("Meal - Interact", false);
            if (WasConsumed(record.choiceId, record.selectedOptionId, "medicine")) SetVisible("Medicine - Interact", false);
            if (WasConsumed(record.choiceId, record.selectedOptionId, "tidy"))
            {
                SetVisible("Cup - Click to Tidy", false);
                SetVisible("Trash Bin Contents", false);
            }
        }
    }

    private static bool WasConsumed(string choiceId, string optionId, string action)
    {
        if (string.IsNullOrEmpty(choiceId) || string.IsNullOrEmpty(optionId)) return false;
        bool actionNode = choiceId.EndsWith("_" + action) || choiceId.Contains("_" + action + "_");
        if (!actionNode || optionId.EndsWith("_skip")) return false;
        // Master Day 3 uses ordered _meal_1/2/3 nodes. The third choice leaves food untouched.
        if (action == "meal") return !optionId.EndsWith("_third") && (optionId == "eat" || optionId.EndsWith("_meal") || optionId.Contains("_meal_"));
        if (action == "medicine") return optionId == "take" || optionId.EndsWith("_medicine") || optionId.Contains("_medicine_");
        return action == "tidy";
    }

    private void CreateNotice()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject textObject = new GameObject("Action Result Notice", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.82f);
        rect.sizeDelta = new Vector2(500, 42);
        notice = textObject.GetComponent<TMP_Text>();
        notice.alignment = TextAlignmentOptions.Center;
        notice.fontSize = 24;
        notice.color = new Color(0.74f, 0.95f, 0.84f);
    }

    private void ClearNotice()
    {
        if (notice != null) notice.text = string.Empty;
    }
}
