using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Leaves small visible traces in the room after selected daily actions.</summary>
public class RoomActionVisuals : MonoBehaviour
{
    private ChoiceManager choiceManager;
    private TMP_Text notice;

    private void Start()
    {
        choiceManager = FindFirstObjectByType<ChoiceManager>();
        if (choiceManager != null) choiceManager.OnChoiceSelected += ApplyResult;
        CreateNotice();
    }

    private void OnDestroy()
    {
        if (choiceManager != null) choiceManager.OnChoiceSelected -= ApplyResult;
    }

    private void ApplyResult(string choiceId, string optionId, int mentalChange)
    {
        if (optionId.Contains("_meal") && !optionId.Contains("skip")) SetVisible("Meal - Interact", false);
        if (optionId.Contains("_medicine") && !optionId.Contains("skip")) SetVisible("Medicine - Interact", false);
        if (optionId.Contains("_tidy") && !optionId.Contains("skip")) SetVisible("Cup - Click to Tidy", false);

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
        GameObject target = GameObject.Find(objectName);
        if (target != null) target.SetActive(visible);
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
