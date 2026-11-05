using TMPro;
using System.Collections.Generic;
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
    private Button primaryButton;
    private Button secondaryButton;
    private Button developerButton;
    private TMP_Text helpText;
    private TMP_Text developerInfo;
    private readonly List<Button> dayButtons = new();
    private readonly List<Button> mentalButtons = new();
    private int selectedDeveloperDay = 1;
    private int selectedDeveloperMental = 100;
    private GraphicRaycaster canvasRaycaster;
    private bool hasStarted;

    public void Initialize(DayManager manager)
    {
        dayManager = manager;
        BuildMenu();
        ShowTitle();
    }

    private void Update()
    {
        if (menuPanel != null && menuPanel.activeSelf)
        {
            HandleMenuPointerInput();
        }

        if (hasStarted && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (menuPanel.activeSelf) Resume(); else Pause();
        }
    }

    private void BuildMenu()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        canvasRaycaster = canvas.GetComponent<GraphicRaycaster>();
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
        primaryButton = CreateButton("새 게임 시작", new Vector2(0, -55), StartNewGame);
        secondaryButton = CreateButton("저장 불러오기", new Vector2(0, -125), LoadGame);
        developerButton = CreateButton("개발용 메뉴", new Vector2(0, -195), ShowDeveloperMenu);
        helpText = CreateText(menuPanel.transform, "Help", new Vector2(0, -275), new Vector2(800, 42), 18);
        helpText.text = "ESC: 일시정지    F5: 저장    F9: 불러오기";
        BuildDeveloperControls();
    }

    private void StartNewGame()
    {
        if (GameStateManager.Instance != null) GameStateManager.Instance.SetState(new GameStateData());
        // DayManager begins once while the title is visible. Discard that hidden
        // pre-title conversation, then deliberately make a fresh DAY 1 checkpoint.
        FindFirstObjectByType<DialogueManager>()?.EndDialogue();
        hasStarted = true;
        menuPanel.SetActive(false);
        SetMenuPointerMode(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        dayManager.BeginDay(1, true);
        FirstPersonPlayerController.UnlockCursor();
    }

    private void LoadGame()
    {
        if (!PrototypeSaveManager.TryLoad()) return;
        FindFirstObjectByType<DialogueManager>()?.EndDialogue();
        hasStarted = true;
        menuPanel.SetActive(false);
        SetMenuPointerMode(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        dayManager.ResumeLoadedDay();
        FirstPersonPlayerController.UnlockCursor();
    }

    private void ShowDeveloperMenu()
    {
        title.text = "개발용 메뉴";
        developerButton.gameObject.SetActive(false);
        helpText.gameObject.SetActive(false);
        developerInfo.gameObject.SetActive(true);
        SetDeveloperLabelsVisible(true);
        foreach (Button button in dayButtons) button.gameObject.SetActive(true);
        foreach (Button button in mentalButtons) button.gameObject.SetActive(true);
        ConfigureButton(primaryButton, "선택한 설정으로 시작", StartDeveloperGame);
        ConfigureButton(secondaryButton, "처음으로", ShowTitle);
        SetButtonPosition(primaryButton, new Vector2(0, -210));
        SetButtonPosition(secondaryButton, new Vector2(0, -275));
        RefreshDeveloperSelection();
    }

    private void StartDeveloperGame()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(new GameStateData
            {
                day = selectedDeveloperDay,
                mentalState = selectedDeveloperMental,
                storyNodeId = "day" + selectedDeveloperDay + "_start"
            });
        }
        FindFirstObjectByType<DialogueManager>()?.EndDialogue();
        hasStarted = true;
        menuPanel.SetActive(false);
        SetMenuPointerMode(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        // Developer starts intentionally preserve the chosen value exactly.
        dayManager.BeginDay(selectedDeveloperDay, true, false);
        FirstPersonPlayerController.UnlockCursor();
    }

    private void Pause()
    {
        title.text = "일시정지";
        ConfigureButton(primaryButton, "계속하기", Resume);
        ConfigureButton(secondaryButton, "저장하기", SaveGame);
        menuPanel.SetActive(true);
        SetMenuPointerMode(true);
        IsMenuOpen = true;
        Time.timeScale = 0f;
        FirstPersonPlayerController.UnlockCursor();
    }

    private void Resume()
    {
        menuPanel.SetActive(false);
        SetMenuPointerMode(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
        FirstPersonPlayerController.LockCursor();
    }

    private void SaveGame()
    {
        PrototypeSaveManager.Save();
        title.text = "저장했습니다";
    }

    private void LateUpdate()
    {
        // Several gameplay HUDs are created in Start, after this menu is built
        // in Awake. Keep an open title/pause menu above them so the title is a
        // real blank entry screen rather than a translucent gameplay overlay.
        if (menuPanel != null && menuPanel.activeSelf)
        {
            menuPanel.transform.SetAsLastSibling();
        }

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

    private void BuildDeveloperControls()
    {
        developerInfo = CreateText(menuPanel.transform, "Developer Info", new Vector2(0, 95), new Vector2(760, 42), 19);
        developerInfo.alignment = TextAlignmentOptions.Center;
        developerInfo.gameObject.SetActive(false);

        CreateText(menuPanel.transform, "Day Label", new Vector2(0, 57), new Vector2(400, 34), 18).text = "시작 일차";
        TMP_Text dayLabel = menuPanel.transform.Find("Day Label")?.GetComponent<TMP_Text>();
        if (dayLabel != null) dayLabel.alignment = TextAlignmentOptions.Center;
        for (int day = 1; day <= 10; day++)
        {
            int chosenDay = day;
            int column = (day - 1) % 5;
            int row = (day - 1) / 5;
            Button button = CreateButton(day + "일차", new Vector2(-168 + column * 84, 17 - row * 48), () =>
            {
                selectedDeveloperDay = chosenDay;
                RefreshDeveloperSelection();
            });
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(72, 38);
            dayButtons.Add(button);
        }

        TMP_Text mentalLabel = CreateText(menuPanel.transform, "Mental Label", new Vector2(0, -92), new Vector2(400, 34), 18);
        mentalLabel.text = "시작 정신력";
        mentalLabel.alignment = TextAlignmentOptions.Center;
        foreach (int mental in new[] { 100, 50, 10 })
        {
            int chosenMental = mental;
            Button button = CreateButton(mental.ToString(), new Vector2((mental == 100 ? -120 : mental == 50 ? 0 : 120), -130), () =>
            {
                selectedDeveloperMental = chosenMental;
                RefreshDeveloperSelection();
            });
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(104, 42);
            mentalButtons.Add(button);
        }

        foreach (Button button in dayButtons) button.gameObject.SetActive(false);
        foreach (Button button in mentalButtons) button.gameObject.SetActive(false);
        SetDeveloperLabelsVisible(false);
    }

    private void RefreshDeveloperSelection()
    {
        developerInfo.text = "시작 일차: " + selectedDeveloperDay + "일차    |    시작 정신력: " + selectedDeveloperMental;
        for (int index = 0; index < dayButtons.Count; index++)
        {
            bool selected = index + 1 == selectedDeveloperDay;
            dayButtons[index].GetComponent<Image>().color = selected ? new Color(0.28f, 0.72f, 0.58f, 1f) : new Color(0.2f, 0.38f, 0.62f, 1f);
        }
        for (int index = 0; index < mentalButtons.Count; index++)
        {
            int value = index == 0 ? 100 : index == 1 ? 50 : 10;
            bool selected = value == selectedDeveloperMental;
            mentalButtons[index].GetComponent<Image>().color = selected ? new Color(0.28f, 0.72f, 0.58f, 1f) : new Color(0.2f, 0.38f, 0.62f, 1f);
        }
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
        ConfigureButton(primaryButton, "새 게임 시작", StartNewGame);
        ConfigureButton(secondaryButton, "저장 불러오기", LoadGame);
        SetButtonPosition(primaryButton, new Vector2(0, -55));
        SetButtonPosition(secondaryButton, new Vector2(0, -125));
        developerButton.gameObject.SetActive(true);
        helpText.gameObject.SetActive(true);
        developerInfo.gameObject.SetActive(false);
        foreach (Button button in dayButtons) button.gameObject.SetActive(false);
        foreach (Button button in mentalButtons) button.gameObject.SetActive(false);
        SetDeveloperLabelsVisible(false);
        IsMenuOpen = true;
        SetMenuPointerMode(true);
        Time.timeScale = 0f;
        FirstPersonPlayerController.UnlockCursor();
    }

    private static void ConfigureButton(Button button, string label, UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = label;
    }

    private static void SetButtonPosition(Button button, Vector2 position)
    {
        if (button != null) button.GetComponent<RectTransform>().anchoredPosition = position;
    }

    private void HandleMenuPointerInput()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        Vector2 pointer = Mouse.current.position.ReadValue();
        if (TryPressButton(primaryButton, pointer) || TryPressButton(secondaryButton, pointer) || TryPressButton(developerButton, pointer)) return;
        foreach (Button button in dayButtons)
        {
            if (TryPressButton(button, pointer)) return;
        }
        foreach (Button button in mentalButtons)
        {
            if (TryPressButton(button, pointer)) return;
        }
    }

    private static bool TryPressButton(Button button, Vector2 pointer)
    {
        if (button == null || !button.gameObject.activeInHierarchy || !button.interactable) return false;
        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (!RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), pointer, camera)) return false;
        button.onClick.Invoke();
        return true;
    }

    private void SetMenuPointerMode(bool menuOpen)
    {
        // The prototype canvas is scaled for its reference resolution. Route
        // title-menu input through the actual button rectangles, matching ChoiceUI.
        if (canvasRaycaster != null) canvasRaycaster.enabled = !menuOpen;
    }

    private void SetDeveloperLabelsVisible(bool visible)
    {
        Transform dayLabel = menuPanel.transform.Find("Day Label");
        Transform mentalLabel = menuPanel.transform.Find("Mental Label");
        if (dayLabel != null) dayLabel.gameObject.SetActive(visible);
        if (mentalLabel != null) mentalLabel.gameObject.SetActive(visible);
    }
}
