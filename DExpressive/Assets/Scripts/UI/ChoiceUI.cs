
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

    private void Start()
    {
        choicePanel.SetActive(false);

        replyButton.onClick.AddListener(OnReply);
        ignoreButton.onClick.AddListener(OnIgnore);
    }

    public void ShowChoices()
    {
        onDialogueChoiceSelected = null;
        displayedDialogueChoices = null;
        questionText.text = "How have you been? Are you okay?";
        SetButtonText(replyButton, "Reply");
        SetButtonText(ignoreButton, "Ignore");
        replyButton.gameObject.SetActive(true);
        ignoreButton.gameObject.SetActive(true);
        choicePanel.SetActive(true);
    }

    public void ShowDialogue(string speakerName, string dialogueText,
        DialogueChoice[] choices, Action<int> choiceCallback)
    {
        if (choices == null || choices.Length < 1 || choices.Length > 2)
        {
            Debug.LogError("ChoiceUI needs one or two dialogue choices.");
            return;
        }

        onDialogueChoiceSelected = choiceCallback;
        displayedDialogueChoices = choices;
        questionText.text = string.IsNullOrWhiteSpace(speakerName)
            ? dialogueText
            : speakerName + "\n" + dialogueText;

        replyButton.gameObject.SetActive(true);
        SetButtonText(replyButton, choices[0].buttonText);

        bool hasSecondChoice = choices.Length == 2;
        ignoreButton.gameObject.SetActive(hasSecondChoice);
        if (hasSecondChoice)
        {
            SetButtonText(ignoreButton, choices[1].buttonText);
        }

        choicePanel.SetActive(true);
    }

    private void OnReply()
    {
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

        Action<int> callback = onDialogueChoiceSelected;
        onDialogueChoiceSelected = null;
        displayedDialogueChoices = null;
        choicePanel.SetActive(false);
        callback?.Invoke(optionIndex);
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
        replyButton.onClick.RemoveListener(OnReply);
        ignoreButton.onClick.RemoveListener(OnIgnore);
    }
}
