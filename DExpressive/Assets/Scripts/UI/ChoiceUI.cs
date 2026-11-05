
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;

public class ChoiceUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button replyButton;
    [SerializeField] private Button ignoreButton;

    [Header("Managers")]
    [SerializeField] private ChoiceManager choiceManager;

    private Action<int> onDialogueChoiceSelected;
    private DialogueChoice[] displayedDialogueChoices;
    private bool initialized;
    private bool isSelecting;
    private bool dialogueInputEnabled;

    public bool IsShowing => choicePanel != null && choicePanel.activeSelf;

    /// <summary>Closes a pending conversation without applying a choice.</summary>
    public void HideChoices()
    {
        if (choicePanel == null) return;
        onDialogueChoiceSelected = null;
        displayedDialogueChoices = null;
        isSelecting = false;
        dialogueInputEnabled = false;
        choicePanel.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        HideChoices();
        ConfigureLayout();
        questionText.raycastTarget = false;
        SetTextRaycast(replyButton, false);
        SetTextRaycast(ignoreButton, false);

        replyButton.onClick.AddListener(OnReply);
        ignoreButton.onClick.AddListener(OnIgnore);
    }

    public void ShowChoices()
    {
        onDialogueChoiceSelected = null;
        displayedDialogueChoices = null;
        questionText.text = "오늘은 어떤 하루였나요?";
        SetButtonText(replyButton, "대답한다");
        SetButtonText(ignoreButton, "지금은 답하지 않는다");
        replyButton.gameObject.SetActive(true);
        ignoreButton.gameObject.SetActive(true);
        choicePanel.SetActive(true);
        dialogueInputEnabled = true;
    }

    public void ShowDialogue(string speakerName, string dialogueText,
        DialogueChoice[] choices, Action<int> choiceCallback, bool heavyPresentation = false)
    {
        Initialize();
        if (choices == null || choices.Length < 1 || choices.Length > 2)
        {
            Debug.LogError("ChoiceUI needs one or two dialogue choices.");
            return;
        }

        onDialogueChoiceSelected = choiceCallback;
        displayedDialogueChoices = choices;
        dialogueInputEnabled = false;
        string prefix = string.IsNullOrWhiteSpace(speakerName) ? string.Empty : speakerName + "\n";
        questionText.text = prefix + dialogueText;

        replyButton.gameObject.SetActive(true);
        SetButtonText(replyButton, choices[0].buttonText);
        ApplyChoiceStyle(replyButton, choices[0].mentalChange, heavyPresentation);

        bool hasSecondChoice = choices.Length == 2;
        ignoreButton.gameObject.SetActive(hasSecondChoice);
        if (hasSecondChoice)
        {
            SetButtonText(ignoreButton, choices[1].buttonText);
            ApplyChoiceStyle(ignoreButton, choices[1].mentalChange, heavyPresentation);
        }

        choicePanel.SetActive(true);
        if (heavyPresentation) StartCoroutine(TypeHeavyDialogue(prefix, dialogueText)); else FocusFirstChoice();
    }

    private void Update()
    {
        // Title/pause menus must own their clicks even if a dialogue was
        // prepared behind them during scene startup.
        if (!IsShowing || PrototypeGameUI.IsMenuOpen || !dialogueInputEnabled) return;

        // Fallback for the prototype canvas: handles clicks by the actual
        // button rectangles even if another UI graphic intercepts a raycast.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pointerPosition = Mouse.current.position.ReadValue();
            if (ContainsPointer(replyButton, pointerPosition))
            {
                OnReply();
                return;
            }
            if (ignoreButton.gameObject.activeSelf && ContainsPointer(ignoreButton, pointerPosition))
            {
                OnIgnore();
                return;
            }
        }

        // Mouse clicks remain the normal control. These keys make choices
        // accessible even if the cursor is hidden or a UI focus is lost.
        if (Keyboard.current != null && (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
        {
            OnReply();
        }
        else if (Keyboard.current != null && ignoreButton.gameObject.activeSelf && Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            OnIgnore();
        }
    }

    private void OnReply()
    {
        if (isSelecting || !IsShowing || !dialogueInputEnabled) return;
        if (onDialogueChoiceSelected != null)
        {
            SelectDialogueOption(0);
            return;
        }

        SelectOption(
            "reply",
            10,
            "friend_conversation"
        );
    }

    private void OnIgnore()
    {
        if (isSelecting || !IsShowing || !dialogueInputEnabled) return;
        if (onDialogueChoiceSelected != null)
        {
            SelectDialogueOption(1);
            return;
        }

        SelectOption(
            "ignore",
            -20,
            "friend_message_ignored"
        );
    }

    private void SelectOption(
        string optionId,
        int mentalChange,
        string nextStoryId)
    {
        choicePanel.SetActive(false);

        choiceManager.SelectChoice(
            "friend_message",
            optionId,
            mentalChange,
            nextStoryId
        );

        Debug.Log("UI 선택 완료: " + optionId);
    }

    private void SelectDialogueOption(int optionIndex)
    {
        if (displayedDialogueChoices == null ||
            optionIndex >= displayedDialogueChoices.Length)
        {
            return;
        }

        isSelecting = true;
        Action<int> callback = onDialogueChoiceSelected;
        onDialogueChoiceSelected = null;
        displayedDialogueChoices = null;
        choicePanel.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
        callback?.Invoke(optionIndex);
        if (!IsShowing)
        {
            FirstPersonPlayerController.LockCursor();
        }
        isSelecting = false;
    }

private void FocusFirstChoice()
    {
        FirstPersonPlayerController.UnlockCursor();
        dialogueInputEnabled = true;
        replyButton.interactable = true;
        ignoreButton.interactable = true;
        EventSystem.current?.SetSelectedGameObject(replyButton.gameObject);
    }

    private IEnumerator TypeHeavyDialogue(string prefix, string dialogueText)
    {
        replyButton.interactable = false;
        ignoreButton.interactable = false;
        questionText.text = prefix;
        for (int index = 0; index < dialogueText.Length; index++)
        {
            questionText.text += dialogueText[index];
            yield return new WaitForSecondsRealtime(0.055f);
            if (index == dialogueText.Length / 2 || dialogueText[index] == '.' || dialogueText[index] == ',') yield return new WaitForSecondsRealtime(0.3f);
        }
        FocusFirstChoice();
    }

    private static void ApplyChoiceStyle(Button button, int mentalChange, bool heavy)
    {
        Image image = button.GetComponent<Image>();
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (!heavy)
        {
            if (image != null) image.color = new Color(0.2f, 0.38f, 0.62f, 1f);
            if (label != null) label.color = Color.white;
            return;
        }
        bool positive = mentalChange > 0;
        if (image != null) image.color = positive ? new Color(0.18f, 0.23f, 0.30f, 0.55f) : new Color(0.48f, 0.11f, 0.14f, 1f);
        if (label != null) label.color = positive ? new Color(0.78f, 0.80f, 0.84f, 0.58f) : new Color(1f, 0.82f, 0.82f, 1f);
    }

    private void ConfigureLayout()
    {
        RectTransform panelRect = choicePanel.GetComponent<RectTransform>();
        if (panelRect != null) panelRect.sizeDelta = new Vector2(650f, 380f);

        Layout(questionText.rectTransform, new Vector2(0f, 105f), new Vector2(570f, 96f));
        Layout(replyButton.GetComponent<RectTransform>(), new Vector2(0f, -10f), new Vector2(440f, 52f));
        Layout(ignoreButton.GetComponent<RectTransform>(), new Vector2(0f, -88f), new Vector2(440f, 52f));
    }

    private static void Layout(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTextRaycast(Button button, bool enabled)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null) text.raycastTarget = enabled;
    }

    private static bool ContainsPointer(Button button, Vector2 screenPosition)
    {
        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(
            button.GetComponent<RectTransform>(), screenPosition, camera);
    }

    private void SetButtonText(Button button, string text)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    private void OnDestroy()
    {
        if (!initialized) return;
        replyButton.onClick.RemoveListener(OnReply);
        ignoreButton.onClick.RemoveListener(OnIgnore);
    }
}
