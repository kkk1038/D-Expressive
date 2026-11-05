using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the current chapter, main objective, and daily interaction checklist.</summary>
public class DayPresentationController : MonoBehaviour
{
    private static readonly string[] Titles = { "", "수레바퀴", "손상", "균열", "늪", "침잠", "낙하", "격돌", "혼란", "비상", "발걸음" };
    private DayManager dayManager;
    private TMP_Text chapterText;
    private TMP_Text goalText;
    private RectTransform cardRect;
    private Vector2 cardBasePosition;

    private void Start()
    {
        dayManager = FindFirstObjectByType<DayManager>();
        BuildCard();
        if (dayManager != null)
        {
            dayManager.OnDayStarted += Refresh;
            dayManager.OnDailyActionRegistered += Refresh;
            Refresh(dayManager.CurrentDay);
        }
    }

    private void OnDestroy()
    {
        if (dayManager == null) return;
        dayManager.OnDayStarted -= Refresh;
        dayManager.OnDailyActionRegistered -= Refresh;
    }

    private void BuildCard()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject card = new GameObject("Daily Presentation", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(canvas.transform, false);
        RectTransform rect = card.GetComponent<RectTransform>();
        cardRect = rect;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-30f, -30f);
        cardBasePosition = rect.anchoredPosition;
        rect.sizeDelta = new Vector2(360f, 350f);
        card.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.13f, 0.7f);

        chapterText = CreateText(card.transform, new Vector2(18f, -16f), new Vector2(-18f, -48f), 24, FontStyles.Bold);
        goalText = CreateText(card.transform, new Vector2(18f, -52f), new Vector2(-18f, -330f), 18, FontStyles.Normal);
    }

    private static TMP_Text CreateText(Transform parent, Vector2 topLeft, Vector2 bottomRight, float size, FontStyles style)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(topLeft.x, bottomRight.y);
        rect.offsetMax = new Vector2(bottomRight.x, topLeft.y);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = size; text.fontStyle = style; text.textWrappingMode = TextWrappingModes.Normal;
        text.color = Color.white;
        return text;
    }

    private void Refresh(int day)
    {
        day = Mathf.Clamp(day, 1, 10);
        if (chapterText != null) chapterText.text = "DAY " + day + "  |  " + Titles[day];
        if (goalText != null)
        {
            string description = day switch
            {
                3 => "몸을 움직이기 힘들다..\n무엇을 할지 신중하게 정하자",
                4 => "해야 할 일은 알지만,\n행동으로 옮기기 어렵다",
                5 => "작은 행동 하나도\n오늘은 큰 선택처럼 느껴진다",
                6 => "일상을 유지할 여유가\n거의 남아 있지 않다",
                7 => "혼자 감당하기 어렵다면\n누군가에게 마음을 전해 보자",
                8 => "후회와 안도감 사이에서\n작은 변화를 이어 가 보자",
                9 => "문 앞까지 왔지만,\n오늘은 아직 열 수 없다",
                _ => string.Empty
            };
            string checklist = string.IsNullOrEmpty(description) ? string.Empty : description + "\n\n";
            checklist += "메인 목표: " + dayManager.GetMainGoalDescription();
            foreach (PrototypeInteractionTarget.InteractionType interaction in dayManager.GetPenaltyInteractions())
            {
                bool complete = dayManager.IsInteractionCompleted(interaction);
                string mark = complete ? "<color=#5CDB78>O</color>" : "<color=#FF5B5B>X</color>";
                checklist += "\n" + DayManager.GetInteractionLabel(interaction) + "  " + mark;
            }
            goalText.text = checklist;
        }
    }

    private void Update()
    {
        if (cardRect == null || dayManager == null) return;
        bool heavyDay = dayManager.CurrentDay == 5 || dayManager.CurrentDay == 6;
        bool needsGoal = !dayManager.HasCompletedMainGoal();
        float pulse = heavyDay && needsGoal ? Mathf.Max(0f, Mathf.Sin(Time.unscaledTime * 2.4f)) : 0f;
        cardRect.anchoredPosition = cardBasePosition + new Vector2(pulse * 2.2f, 0f);
    }
}
