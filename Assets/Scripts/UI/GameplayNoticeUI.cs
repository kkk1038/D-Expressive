using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Small non-blocking notice used for interaction and objective feedback.</summary>
public class GameplayNoticeUI : MonoBehaviour
{
    private static GameplayNoticeUI instance;
    private TMP_Text label;
    private GameObject panel;
    private Coroutine hideRoutine;

    public static void Show(string message, float duration = 2f)
    {
        if (instance == null)
        {
            GameObject noticeObject = new GameObject("Gameplay Notice");
            instance = noticeObject.AddComponent<GameplayNoticeUI>();
        }
        instance.Display(message, duration);
    }

    private void Display(string message, float duration)
    {
        EnsureUI();
        if (label == null) return;
        label.text = message;
        panel.SetActive(true);
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    private void EnsureUI()
    {
        if (label != null) return;
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        panel = new GameObject("Notice Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -205f);
        panelRect.sizeDelta = new Vector2(650f, 88f);
        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.04f, 0.06f, 0.12f, 0.92f);
        image.raycastTarget = false;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 10f);
        textRect.offsetMax = new Vector2(-20f, -10f);
        label = textObject.GetComponent<TMP_Text>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 23;
        label.color = Color.white;
        label.raycastTarget = false;
        panel.SetActive(false);
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (panel != null) panel.SetActive(false);
    }
}
