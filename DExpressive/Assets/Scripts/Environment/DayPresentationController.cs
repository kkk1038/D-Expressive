using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the current chapter's mood and one small achievable action.</summary>
public class DayPresentationController : MonoBehaviour
{
    private static readonly string[] Titles = { "", "수레바퀴", "손상", "균열", "늪", "침잠", "낙하", "격돌", "혼란", "비상", "발걸음" };
    private static readonly string[] Goals =
    {
        "", "방 안에서 할 수 있는 일을 하나 해 보자.", "아주 작은 일 하나만 해도 충분하다.",
        "식사나 세수처럼 몸을 돌보는 일을 해 보자.", "연락 하나 또는 정리 하나를 선택해 보자.",
        "거울을 보고, 오늘의 자신을 외면하지 말자.", "짧은 답장 하나를 남겨 보자.",
        "혼자 견디기 어렵다면 누군가에게 연락해 보자.", "어제의 대화에 짧게라도 답해 보자.",
        "문 앞에서 잠시 숨을 고르고 주변을 살펴보자.", "오늘의 준비를 하나만 마쳐 보자."
    };

    private DayManager dayManager;
    private TMP_Text chapterText;
    private TMP_Text goalText;

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
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-30f, -30f);
        rect.sizeDelta = new Vector2(360f, 120f);
        card.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.13f, 0.7f);

        chapterText = CreateText(card.transform, new Vector2(18f, -16f), new Vector2(-18f, -48f), 24, FontStyles.Bold);
        goalText = CreateText(card.transform, new Vector2(18f, -52f), new Vector2(-18f, -108f), 17, FontStyles.Normal);
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
        bool acted = GameStateManager.Instance != null && GameStateManager.Instance.CurrentState.completedInteractions.Contains("day_" + day + "_action_taken");
        if (chapterText != null) chapterText.text = "DAY " + day + "  |  " + Titles[day];
        if (goalText != null) goalText.text = acted ? "오늘의 작은 행동을 해냈습니다." : "오늘의 작은 목표\n" + Goals[day];
    }
}
