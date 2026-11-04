using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Brief fade-to-black transition when the player ends a day in bed.</summary>
public class SleepTransitionController : MonoBehaviour
{
    private DayManager dayManager;
    private Image fadeImage;
    private TMP_Text label;
    private Coroutine transition;

    private void Start()
    {
        dayManager = FindFirstObjectByType<DayManager>();
        if (dayManager != null) dayManager.OnDayEnded += PlayTransition;
        CreateUI();
    }

    private void OnDestroy()
    {
        if (dayManager != null) dayManager.OnDayEnded -= PlayTransition;
    }

    private void CreateUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject overlay = new GameObject("Sleep Transition", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        fadeImage = overlay.GetComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        GameObject labelObject = new GameObject("Sleep Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(overlay.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(680f, 80f);
        label = labelObject.GetComponent<TMP_Text>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 34;
        label.color = Color.white;
    }

    private void PlayTransition(int endingDay)
    {
        if (fadeImage == null) return;
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(FadeToNextDay(endingDay + 1));
    }

    private IEnumerator FadeToNextDay(int nextDay)
    {
        fadeImage.transform.SetAsLastSibling();
        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            SetAlpha(t / 0.35f);
            yield return null;
        }
        SetAlpha(1f);
        label.text = "잠시 눈을 감았다...\nDAY - " + nextDay;
        yield return new WaitForSeconds(0.75f);
        label.text = string.Empty;
        for (float t = 0f; t < 0.45f; t += Time.deltaTime)
        {
            SetAlpha(1f - t / 0.45f);
            yield return null;
        }
        SetAlpha(0f);
        transition = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
