using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Runtime title, pause and save menu for the prototype scene.</summary>
public class PrototypeGameUI : MonoBehaviour
{
    public static bool IsMenuOpen { get; private set; }
    private DayManager dayManager;
    private GameObject menuPanel;
    private TMP_Text title;
    private bool hasStarted;

    public void Initialize(DayManager manager)
    {
        dayManager = manager;
        BuildMenu();
        ShowTitle();
    }

    private void Update()
    {
        if (hasStarted && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (menuPanel.activeSelf) Resume(); else Pause();
        }
    }

    private void BuildMenu()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        menuPanel = new GameObject("Game Menu", typeof(RectTransform), typeof(Image)).gameObject;
        menuPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;
        // The title screen deliberately hides the prototype room completely.
        menuPanel.GetComponent<Image>().color = new Color(0.025f, 0.035f, 0.07f, 1f);

        title = CreateText(menuPanel.transform, "Title", new Vector2(0, 125), new Vector2(900, 90), 54);
        title.alignment = TextAlignmentOptions.Center;
        CreateText(menuPanel.transform, "Subtitle", new Vector2(0, 45), new Vector2(900, 48), 22).text = "D-EXPRESSIVE";
        CreateButton("새 게임 시작", new Vector2(0, -55), StartNewGame);
        CreateButton("저장 불러오기", new Vector2(0, -125), LoadGame);
        CreateText(menuPanel.transform, "Help", new Vector2(0, -235), new Vector2(800, 42), 18).text = "ESC: 일시정지    F5: 저장    F9: 불러오기";
    }

    private void StartNewGame()
    {
        hasStarted = true;
        menuPanel.SetActive(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        ChoiceUI choiceUI = FindFirstObjectByType<ChoiceUI>();
        if (choiceUI != null && choiceUI.IsShowing) FirstPersonPlayerController.UnlockCursor();
        else FirstPersonPlayerController.LockCursor();
    }

    private void LoadGame()
    {
        if (!PrototypeSaveManager.TryLoad()) return;
        hasStarted = true;
        menuPanel.SetActive(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        dayManager.ResumeLoadedDay();
        FirstPersonPlayerController.UnlockCursor();
    }

    private void Pause()
    {
        title.text = "일시정지";
        menuPanel.SetActive(true);
        IsMenuOpen = true;
        Time.timeScale = 0f;
        FirstPersonPlayerController.UnlockCursor();
    }

    private void Resume()
    {
        menuPanel.SetActive(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        FirstPersonPlayerController.LockCursor();
    }

    private void LateUpdate()
    {
        if (!hasStarted || Keyboard.current == null) return;
        if (Keyboard.current.f5Key.wasPressedThisFrame) PrototypeSaveManager.Save();
        if (Keyboard.current.f9Key.wasPressedThisFrame && PrototypeSaveManager.TryLoad()) dayManager.ResumeLoadedDay();
    }

    private Button CreateButton(string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject button = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        button.transform.SetParent(menuPanel.transform, false);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = new Vector2(340, 52);
        button.GetComponent<Image>().color = new Color(0.2f, 0.38f, 0.62f, 1f);
        Button component = button.GetComponent<Button>(); component.onClick.AddListener(action);
        TMP_Text text = CreateText(button.transform, "Label", Vector2.zero, new Vector2(340, 52), 22);
        text.text = label; text.alignment = TextAlignmentOptions.Center;
        return component;
    }

    private static TMP_Text CreateText(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = fontSize; text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    private void ShowTitle()
    {
        title.text = "D-EXPRESSIVE";
        IsMenuOpen = true;
        Time.timeScale = 0f;
        FirstPersonPlayerController.UnlockCursor();
    }
}
