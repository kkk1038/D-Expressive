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
        float mental = state.mentalState / 100f;
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

        if (mentalOverlay != null)
        {
            float distress = Mathf.Clamp01((45f - state.mentalState) / 45f);
            mentalOverlay.color = new Color(0.24f, 0.02f, 0.08f, distress * 0.32f);
        }
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
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
