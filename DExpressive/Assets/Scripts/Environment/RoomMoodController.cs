using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Changes the prototype room's lighting by day and mental state, and plays the rollback transition.</summary>
public class RoomMoodController : MonoBehaviour
{
    [SerializeField] private RollbackManager rollbackManager;
    [SerializeField] private float rollbackFadeDuration = 0.45f;

    private Light roomLight;
    private Renderer windowRenderer;
    private Material windowMoodMaterial;
    private Image fadeImage;
    private TMP_Text rollbackLabel;
    private Image mentalOverlay;
    private TMP_Text mentalWarningLabel;
    private float day7RecoveryProgress;
    private bool wasDay7Recovering;

    private void Awake()
    {
        if (rollbackManager == null) rollbackManager = FindFirstObjectByType<RollbackManager>();
    }

    private void OnEnable()
    {
        if (rollbackManager != null) rollbackManager.OnRollbackCompleted += PlayRollbackTransition;
    }

    private void OnDisable()
    {
        if (rollbackManager != null) rollbackManager.OnRollbackCompleted -= PlayRollbackTransition;
    }

    private void Start()
    {
        roomLight = GameObject.Find("Prototype Room Light")?.GetComponent<Light>();
        var windowObject = GameObject.Find("Window - Click to Look Outside");
        windowRenderer = windowObject?.GetComponent<Renderer>();
        if (windowObject != null && windowRenderer == null)
        {
            foreach (var renderer in windowObject.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;
                foreach (var material in renderer.materials)
                    if (material.name.StartsWith("glass")) { windowRenderer = renderer; windowMoodMaterial = material; break; }
                if (windowMoodMaterial != null) break;
            }
        }
        if (windowMoodMaterial == null && windowRenderer != null) windowMoodMaterial = windowRenderer.material;
        CreateTransitionUI();
        CreateMentalOverlay();
        ApplyMood();
    }

    private void Update()
    {
        ApplyMood();
    }

private void ApplyMood()
    {
        if (GameStateManager.Instance == null || roomLight == null || windowRenderer == null) return;

        GameStateData state = GameStateManager.Instance.CurrentState;
        int effectMental = state.dayStartMentalState > 0 ? state.dayStartMentalState : state.mentalState;
        if (state.day == 6) effectMental = 10;
        bool isDay7Recovering = state.day == 7 && state.storyFlags.Contains("day7_contact_recovery");
        if (isDay7Recovering)
        {
            day7RecoveryProgress = Mathf.MoveTowards(day7RecoveryProgress, 1f, Time.unscaledDeltaTime / 5f);
        }
        else if (wasDay7Recovering)
        {
            day7RecoveryProgress = 0f;
        }
        wasDay7Recovering = isDay7Recovering;

        float displayedMental = Mathf.Lerp(effectMental, 100f, day7RecoveryProgress);
        float mental = displayedMental / 100f;
        bool isLaterDay = state.day >= 2;

        Color brightLight = isLaterDay ? new Color(0.66f, 0.70f, 0.92f) : new Color(1f, 0.78f, 0.56f);
        Color lowMentalLight = new Color(0.25f, 0.33f, 0.55f);
        roomLight.color = Color.Lerp(lowMentalLight, brightLight, mental);
        roomLight.intensity = Mathf.Lerp(2.4f, isLaterDay ? 5.5f : 8.5f, mental);

        Color dayWindow = isLaterDay ? new Color(0.20f, 0.31f, 0.56f) : new Color(0.20f, 0.55f, 0.85f);
        Color lowMentalWindow = new Color(0.10f, 0.12f, 0.23f);
        Color windowColor = Color.Lerp(lowMentalWindow, dayWindow, mental);
        windowColor.a = windowMoodMaterial.color.a;
        windowMoodMaterial.color = windowColor;

        RenderSettings.ambientLight = Color.Lerp(new Color(0.04f, 0.05f, 0.10f), new Color(0.35f, 0.35f, 0.40f), mental);

        ApplyMentalStateEffect(Mathf.RoundToInt(displayedMental), isDay7Recovering);
    }

    private void PlayRollbackTransition()
    {
        StopAllCoroutines();
        StartCoroutine(RollbackTransition());
    }

    private IEnumerator RollbackTransition()
    {
        if (fadeImage == null) yield break;

        rollbackLabel.text = GameStateManager.Instance.CurrentState.day + "일 차로 돌아갑니다";
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(0.65f);
        ApplyMood();
        yield return Fade(1f, 0f);
        rollbackLabel.text = string.Empty;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < rollbackFadeDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, elapsed / rollbackFadeDuration));
            yield return null;
        }
        SetFadeAlpha(to);
    }

    private void CreateTransitionUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject overlay = new GameObject("Rollback Overlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        fadeImage = overlay.GetComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;

        GameObject labelObject = new GameObject("Rollback Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(overlay.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(700, 100);
        rollbackLabel = labelObject.GetComponent<TextMeshProUGUI>();
        rollbackLabel.alignment = TextAlignmentOptions.Center;
        rollbackLabel.fontSize = 38;
        rollbackLabel.color = Color.white;
    }

    private void CreateMentalOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject overlay = new GameObject("Mental State Overlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        mentalOverlay = overlay.GetComponent<Image>();
        mentalOverlay.raycastTarget = false;

        GameObject warningObject = new GameObject("Mental State Warning", typeof(RectTransform), typeof(TextMeshProUGUI));
        warningObject.transform.SetParent(overlay.transform, false);
        RectTransform warningRect = warningObject.GetComponent<RectTransform>();
        warningRect.anchorMin = new Vector2(0.5f, 1f);
        warningRect.anchorMax = new Vector2(0.5f, 1f);
        warningRect.pivot = new Vector2(0.5f, 1f);
        warningRect.anchoredPosition = new Vector2(0f, -95f);
        warningRect.sizeDelta = new Vector2(700f, 50f);
        mentalWarningLabel = warningObject.GetComponent<TextMeshProUGUI>();
        mentalWarningLabel.alignment = TextAlignmentOptions.Center;
        mentalWarningLabel.fontSize = 22;
        mentalWarningLabel.fontStyle = FontStyles.Bold;
        mentalWarningLabel.raycastTarget = false;
    }

    private void ApplyMentalStateEffect(int mentalState, bool hideWarning)
    {
        if (mentalOverlay == null) return;

        // These thresholds are deliberately discrete: each drop is immediately legible.
        Color overlayColor = Color.clear;
        string warning = string.Empty;
        if (mentalState <= 10)
        {
            float pulse = 0.04f * Mathf.Sin(Time.unscaledTime * 3.5f);
            overlayColor = new Color(0.05f, 0f, 0.02f, 0.60f + pulse);
            warning = "정신력이 바닥에 닿았다";
        }
        else if (mentalState <= 30)
        {
            float pulse = 0.025f * Mathf.Sin(Time.unscaledTime * 2.2f);
            overlayColor = new Color(0.28f, 0.01f, 0.06f, 0.38f + pulse);
            warning = "몸과 생각이 무겁다";
        }
        else if (mentalState <= 50)
        {
            overlayColor = new Color(0.08f, 0.12f, 0.24f, 0.22f);
            warning = "조금씩 지쳐 간다";
        }

        mentalOverlay.color = overlayColor;
        if (mentalWarningLabel != null)
        {
            mentalWarningLabel.text = hideWarning ? string.Empty : warning;
            mentalWarningLabel.color = mentalState <= 10
                ? new Color(1f, 0.48f, 0.48f, 0.9f)
                : new Color(0.86f, 0.82f, 0.88f, 0.82f);
        }
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
