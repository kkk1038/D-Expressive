using TMPro;
using UnityEngine;

/// <summary>Runtime HUD for the mental-state value stored in GameStateData.</summary>
public class MentalStateHUD : MonoBehaviour
{
    private TMP_Text label;

    private void Start()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject labelObject = new GameObject("Mental State HUD", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(28, -28);
        rect.sizeDelta = new Vector2(300, 52);
        label = labelObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = 25;
    }

    private void Update()
    {
        if (label == null || GameStateManager.Instance == null) return;

        int value = GameStateManager.Instance.CurrentState.mentalState;
        label.text = "정신력  " + value + " / 100";
        label.color = Color.Lerp(new Color(0.9f, 0.15f, 0.15f), new Color(0.4f, 1f, 0.6f), value / 100f);
    }
}
